using Kongroo.Catalog.Contracts;
using Kongroo.Payments.Contracts;
using Kongroo.Payments.Domain;
using Kongroo.Payments.Infrastructure;
using Kongroo.Payments.IntegrationTests.Support;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Kongroo.Payments.IntegrationTests;

[Collection(nameof(PaymentsCollection))]
public sealed class PaymentProcessingTests(PaymentsFixture fixture)
{
    [Theory]
    [InlineData(500.00, true, "Approved")]
    [InlineData(1500.00, false, "Rejected")]
    public async Task Consume_WithOrderPlacedEvent_ShouldProcessPaymentAndPublishResult(
        decimal amount,
        bool expectedApproved,
        string expectedStatus
    )
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

        var received = new TaskCompletionSource<PaymentProcessedIntegrationEvent>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var endpoint = bus.ConnectReceiveEndpoint(
            $"test-payment-processed-{Guid.CreateVersion7():N}",
            configurator =>
                configurator.Handler<PaymentProcessedIntegrationEvent>(context =>
                {
                    received.TrySetResult(context.Message);
                    return Task.CompletedTask;
                })
        );
        await endpoint.Ready;

        var orderId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();
        await bus.Publish(
            new OrderPlacedIntegrationEvent(orderId, customerId, "grace@example.com", "Grace Hopper", amount, "BRL"),
            cancellationToken
        );

        var published = await received.Task.WaitAsync(TimeSpan.FromSeconds(60), cancellationToken);
        published.OrderId.ShouldBe(orderId);
        published.UserId.ShouldBe(customerId);
        published.Email.ShouldBe("grace@example.com");
        published.Approved.ShouldBe(expectedApproved);

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

        using var verifyScope = factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
        var persisted = await verifyContext.Payments.SingleAsync(
            payment => payment.OrderId == OrderId.From(orderId),
            cancellationToken
        );
        persisted.Status.ToString().ShouldBe(expectedStatus);

        await endpoint.StopAsync(cancellationToken);
    }

    [Fact]
    public async Task Consume_WhenOrderPlacedEventIsDuplicated_ShouldCreateSinglePayment()
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
        var message = new OrderPlacedIntegrationEvent(
            orderId,
            customerId,
            "grace@example.com",
            "Grace Hopper",
            250.00m,
            "BRL"
        );

        await bus.Publish(message, cancellationToken);
        await bus.Publish(message, cancellationToken);

        await TestPolling.WaitUntilAsync(
            async () =>
            {
                using var scope = factory.Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
                return await context.Payments.CountAsync(
                        payment => payment.OrderId == OrderId.From(orderId),
                        cancellationToken
                    ) >= 1;
            },
            cancellationToken
        );

        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

        using var verifyScope = factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
        var count = await verifyContext.Payments.CountAsync(
            payment => payment.OrderId == OrderId.From(orderId),
            cancellationToken
        );
        count.ShouldBe(1);
    }
}
