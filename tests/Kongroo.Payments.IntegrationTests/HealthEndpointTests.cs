using System.Net;
using Kongroo.Payments.IntegrationTests.Support;
using Shouldly;

namespace Kongroo.Payments.IntegrationTests;

[Collection(nameof(PaymentsCollection))]
public sealed class HealthEndpointTests(PaymentsFixture fixture)
{
    [Fact]
    public async Task GetHealth_ShouldReturnOk()
    {
        await using var factory = new PaymentsApiFactory(
            fixture.BrokerHost,
            fixture.BrokerPort,
            fixture.DbConnectionString
        );
        using var client = factory.CreateClient();

        await TestPolling.WaitForHealthyAsync(client, TestContext.Current.CancellationToken);

        using var response = await client.GetAsync("/health", TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetOpenApiDocument_InDevelopment_ShouldReturnOk()
    {
        await using var factory = new PaymentsApiFactory(
            fixture.BrokerHost,
            fixture.BrokerPort,
            fixture.DbConnectionString
        );
        using var client = factory.CreateClient();
        await TestPolling.WaitForHealthyAsync(client, TestContext.Current.CancellationToken);

        using var response = await client.GetAsync("/openapi/v1.json", TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
