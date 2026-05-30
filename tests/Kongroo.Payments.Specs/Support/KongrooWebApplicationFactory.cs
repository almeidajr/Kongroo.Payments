using Microsoft.AspNetCore.Mvc.Testing;

namespace Kongroo.Payments.Specs.Support;

public sealed class KongrooWebApplicationFactory : WebApplicationFactory<Program>
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
                    ["OutboxProcessing:PollingInterval"] = "00:10:00",
                    ["OutboxProcessing:BatchSize"] = "1",
                };

                configurationBuilder.AddInMemoryCollection(testConfiguration);
            }
        );

        builder.ConfigureLogging(loggingBuilder => loggingBuilder.ClearProviders());
    }
}
