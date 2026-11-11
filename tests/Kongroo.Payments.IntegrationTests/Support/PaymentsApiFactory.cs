using System.Globalization;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Kongroo.Payments.IntegrationTests.Support;

public sealed class PaymentsApiFactory(
    string brokerHost,
    int brokerPort,
    string brokerPassword,
    string dbConnectionString
) : WebApplicationFactory<Program>
{
    public const string Issuer = "Kongroo.Payments.IntegrationTests";
    public const string Audience = "Kongroo.Payments.IntegrationTests";
    public static readonly string SigningKey = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

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
                    ["Payments:ApprovalLimit"] = "1000.00",
                    ["ConnectionStrings:Database"] = dbConnectionString,
                    ["RabbitMq:Host"] = brokerHost,
                    ["RabbitMq:Port"] = brokerPort.ToString(CultureInfo.InvariantCulture),
                    ["RabbitMq:User"] = PaymentsFixture.BrokerUsername,
                    ["RabbitMq:Pass"] = brokerPassword,
                };

                configurationBuilder.AddInMemoryCollection(testConfiguration);
            }
        );

        builder.ConfigureLogging(loggingBuilder => loggingBuilder.ClearProviders());
    }
}
