using Kongroo.Payments.Contracts;
using MassTransit;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace Kongroo.Payments.Specs.Support;

public static class SpecsEnvironment
{
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static PostgreSqlContainer? _database;
    private static RabbitMqContainer? _broker;
    private static KongrooWebApplicationFactory? _factory;
    private static HostReceiveEndpointHandle? _captureEndpoint;

    public static KongrooWebApplicationFactory Factory =>
        _factory ?? throw new InvalidOperationException("The specs environment has not been started.");

    public static IBus Bus => Factory.Services.GetRequiredService<IBus>();

    public static async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_factory is not null)
        {
            return;
        }

        await Gate.WaitAsync(cancellationToken);
        try
        {
            if (_factory is not null)
            {
                return;
            }

            _database = new PostgreSqlBuilder("postgres:18.3")
                .WithDatabase("kongroo_payments")
                .WithUsername("kongroo")
                .WithPassword("development")
                .Build();
            _broker = new RabbitMqBuilder("rabbitmq:4-management")
                .WithUsername("kongroo")
                .WithPassword("development")
                .Build();

            await _database.StartAsync(cancellationToken);
            await _broker.StartAsync(cancellationToken);

            _factory = new KongrooWebApplicationFactory(
                _broker.Hostname,
                _broker.GetMappedPublicPort(5672),
                _database.GetConnectionString()
            );

            using var client = _factory.CreateClient();
            // The MassTransit bus connects asynchronously; poll until the host reports healthy.
            await WaitForHealthyAsync(client, cancellationToken);

            _captureEndpoint = Bus.ConnectReceiveEndpoint(
                "specs-payment-processed",
                configurator =>
                    configurator.Handler<PaymentProcessedIntegrationEvent>(context =>
                    {
                        PublishedEvents.PaymentProcessed.Add(context.Message);
                        return Task.CompletedTask;
                    })
            );
            await _captureEndpoint.Ready;
        }
        finally
        {
            Gate.Release();
        }
    }

    public static async Task StopAsync()
    {
        if (_captureEndpoint is not null)
        {
            await _captureEndpoint.StopAsync();
            _captureEndpoint = null;
        }

        if (_factory is not null)
        {
            await _factory.DisposeAsync();
            _factory = null;
        }

        if (_broker is not null)
        {
            await _broker.DisposeAsync();
            _broker = null;
        }

        if (_database is not null)
        {
            await _database.DisposeAsync();
            _database = null;
        }
    }

    private static async Task WaitForHealthyAsync(HttpClient client, CancellationToken cancellationToken)
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(60);
        while (true)
        {
            using var response = await client.GetAsync("/health", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            if (DateTimeOffset.UtcNow >= deadline)
            {
                response.EnsureSuccessStatusCode();
            }

            await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);
        }
    }
}
