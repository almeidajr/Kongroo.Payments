using Microsoft.AspNetCore.Mvc.Testing;

namespace Kongroo.Payments.Specs.Support;

public sealed class ApiScenarioContext : IDisposable
{
    private HttpClient? _client;

    public HttpClient Client =>
        _client ??= SpecsEnvironment.Factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false }
        );

    public HttpResponseMessage? LastResponse { get; private set; }

    public string LastResponseContent { get; private set; } = string.Empty;

    public void SetLastResponse(HttpResponseMessage response, string responseContent)
    {
        LastResponse?.Dispose();
        LastResponse = response;
        LastResponseContent = responseContent;
    }

    public void Dispose()
    {
        LastResponse?.Dispose();
        _client?.Dispose();
    }
}
