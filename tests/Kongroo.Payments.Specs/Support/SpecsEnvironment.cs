namespace Kongroo.Payments.Specs.Support;

public static class SpecsEnvironment
{
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static KongrooWebApplicationFactory? _factory;

    public static KongrooWebApplicationFactory Factory =>
        _factory ?? throw new InvalidOperationException("The specs environment has not been started.");

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

            _factory = new KongrooWebApplicationFactory();

            using var client = _factory.CreateClient();
            using var response = await client.GetAsync("/health", cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        finally
        {
            Gate.Release();
        }
    }

    public static async Task StopAsync()
    {
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
            _factory = null;
        }
    }
}
