using System.Globalization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Kongroo.Payments.IntegrationTests.Support;

public sealed class PaymentsApiFactory(string brokerHost, int brokerPort, string dbConnectionString)
    : WebApplicationFactory<Program>
{
    public const string Issuer = "Kongroo.Payments.IntegrationTests";
    public const string Audience = "Kongroo.Payments.IntegrationTests";
    public const string SigningKey = "Kongroo.Payments.IntegrationTests.SigningKey.For.Tests";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration(
            (_, configurationBuilder) =>
            {
                var testConfiguration = new Dictionary<string, string?>
                {
                    ["Jwt:Issuer"] = Issuer,
                    ["Jwt:Audience"] = Audience,
                    ["Jwt:SigningKey"] = SigningKey,
                    ["OutboxProcessing:PollingInterval"] = "00:00:01",
                    ["OutboxProcessing:BatchSize"] = "20",
                    ["Payments:ApprovalLimit"] = "1000.00",
                    ["ConnectionStrings:Database"] = dbConnectionString,
                    ["RabbitMq:Host"] = brokerHost,
                    ["RabbitMq:Port"] = brokerPort.ToString(CultureInfo.InvariantCulture),
                    ["RabbitMq:User"] = "kongroo",
                    ["RabbitMq:Pass"] = "development",
                };

                configurationBuilder.AddInMemoryCollection(testConfiguration);
            }
        );

        builder.ConfigureLogging(loggingBuilder => loggingBuilder.ClearProviders());
    }
}
