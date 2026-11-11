using System.Globalization;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Kongroo.Payments.IntegrationTests.Support;

public sealed class PaymentsApiFactory(PaymentsFixture fixture) : WebApplicationFactory<Program>
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
                    ["ConnectionStrings:Database"] = fixture.DbConnectionString,
                    ["RabbitMq:Host"] = fixture.BrokerHost,
                    ["RabbitMq:Port"] = fixture.BrokerPort.ToString(CultureInfo.InvariantCulture),
                    ["RabbitMq:User"] = PaymentsFixture.BrokerUsername,
                    ["RabbitMq:Pass"] = PaymentsFixture.BrokerPassword,
                };

                configurationBuilder.AddInMemoryCollection(testConfiguration);
            }
        );

        builder.ConfigureLogging(loggingBuilder => loggingBuilder.ClearProviders());
    }
}
