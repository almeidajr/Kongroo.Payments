using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Kongroo.Catalog.Contracts;
using Kongroo.Payments.Application;
using Kongroo.Payments.Domain;
using Kongroo.Payments.Infrastructure;
using Kongroo.Payments.IntegrationTests.Support;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Kongroo.Payments.IntegrationTests;

[Collection(nameof(PaymentsCollection))]
public sealed class PaymentQueryTests(PaymentsFixture fixture)
{
    [Fact]
    public async Task GetPaymentByOrderId_WithCallerPayment_ShouldReturnCallersPayment()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new PaymentsApiFactory(
            fixture.BrokerHost,
            fixture.BrokerPort,
            fixture.DbConnectionString
        );
        using var client = factory.CreateClient();
        await TestPolling.WaitForHealthyAsync(client, cancellationToken);

        var bus = factory.Services.GetRequiredService<IBus>();
        var orderId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();
        await bus.Publish(
            new OrderPlacedIntegrationEvent(
                orderId,
                customerId,
                "ada@example.com",
                "Ada Lovelace",
                42.00m,
                "USD",
                [new OrderPlacedLine(Guid.CreateVersion7(), 42.00m)]
            ),
            cancellationToken
        );

        await TestPolling.WaitUntilAsync(
            async () =>
            {
                using var scope = factory.Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
                return await context.Payments.AnyAsync(
                    payment => payment.OrderId == OrderId.From(orderId),
                    cancellationToken
                );
            },
            cancellationToken
        );

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            TestTokens.ForCustomer(customerId)
        );

        var response = await client.GetFromJsonAsync<PaymentResponse>($"/{orderId}", cancellationToken);

        response.ShouldNotBeNull();
        response.OrderId.ShouldBe(orderId);
        response.CustomerId.ShouldBe(customerId);
        response.Status.ShouldBe("Approved");
        response.Currency.ShouldBe("USD");
    }

    [Fact]
    public async Task GetPaymentByOrderId_WhenPaymentBelongsToOtherCustomer_ShouldReturnNotFound()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new PaymentsApiFactory(
            fixture.BrokerHost,
            fixture.BrokerPort,
            fixture.DbConnectionString
        );
        using var client = factory.CreateClient();
        await TestPolling.WaitForHealthyAsync(client, cancellationToken);

        var bus = factory.Services.GetRequiredService<IBus>();
        var orderId = Guid.CreateVersion7();
        var ownerId = Guid.CreateVersion7();
        await bus.Publish(
            new OrderPlacedIntegrationEvent(
                orderId,
                ownerId,
                "ada@example.com",
                "Ada Lovelace",
                42.00m,
                "USD",
                [new OrderPlacedLine(Guid.CreateVersion7(), 42.00m)]
            ),
            cancellationToken
        );

        await TestPolling.WaitUntilAsync(
            async () =>
            {
                using var scope = factory.Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
                return await context.Payments.AnyAsync(
                    payment => payment.OrderId == OrderId.From(orderId),
                    cancellationToken
                );
            },
            cancellationToken
        );

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            TestTokens.ForCustomer(Guid.CreateVersion7())
        );

        using var response = await client.GetAsync($"/{orderId}", cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
