using System.Globalization;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Kongroo.Payments.Specs.Support;

public sealed class KongrooWebApplicationFactory(string brokerHost, int brokerPort, string dbConnectionString)
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration(
            (_, configurationBuilder) =>
            {
                var testConfiguration = new Dictionary<string, string?>
                {
                    ["Jwt:Issuer"] = "Kongroo.Payments.Specs",
                    ["Jwt:Audience"] = "Kongroo.Payments.Specs",
                    ["Jwt:SigningKey"] = "Kongroo.Payments.Specs.SigningKey.For.Bdd.Tests",
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
