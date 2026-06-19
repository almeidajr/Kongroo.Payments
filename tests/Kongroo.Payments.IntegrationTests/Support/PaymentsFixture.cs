using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace Kongroo.Payments.IntegrationTests.Support;

public sealed class PaymentsFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:18.3")
        .WithDatabase("kongroo_payments")
        .WithUsername("kongroo")
        .WithPassword("development")
        .Build();

    private readonly RabbitMqContainer _broker = new RabbitMqBuilder("rabbitmq:4-management")
        .WithUsername("kongroo")
        .WithPassword("development")
        .Build();

    public string DbConnectionString => _database.GetConnectionString();

    public string BrokerHost => _broker.Hostname;

    public int BrokerPort => _broker.GetMappedPublicPort(5672);

    public async ValueTask InitializeAsync()
    {
        await _database.StartAsync();
        await _broker.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _broker.DisposeAsync();
        await _database.DisposeAsync();
    }
}

[CollectionDefinition(nameof(PaymentsCollection))]
public sealed class PaymentsCollection : ICollectionFixture<PaymentsFixture>;
