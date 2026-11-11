using System.Globalization;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Kongroo.Payments.Specs.Support;

public sealed class KongrooWebApplicationFactory(
    string brokerHost,
    int brokerPort,
    string brokerUsername,
    string brokerPassword,
    string dbConnectionString
) : WebApplicationFactory<Program>
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
                    ["Jwt:SigningKey"] = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)),
                    ["Payments:ApprovalLimit"] = "1000.00",
                    ["ConnectionStrings:Database"] = dbConnectionString,
                    ["RabbitMq:Host"] = brokerHost,
                    ["RabbitMq:Port"] = brokerPort.ToString(CultureInfo.InvariantCulture),
                    ["RabbitMq:User"] = brokerUsername,
                    ["RabbitMq:Pass"] = brokerPassword,
                };

                configurationBuilder.AddInMemoryCollection(testConfiguration);
            }
        );

        builder.ConfigureLogging(loggingBuilder => loggingBuilder.ClearProviders());
    }
}
