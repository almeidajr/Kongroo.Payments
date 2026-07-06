using Kongroo.Payments.Application;
using Kongroo.Payments.Contracts;
using Kongroo.Payments.Domain;
using MassTransit;
using NSubstitute;

namespace Kongroo.Payments.UnitTests.Application;

public sealed class PaymentProcessedDomainEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithPaymentProcessedDomainEvent_ShouldPublishIntegrationEventWithMappedFields()
    {
        var publishEndpoint = Substitute.For<IPublishEndpoint>();
        var handler = new PaymentProcessedDomainEventHandler(publishEndpoint);

        var orderId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();
        var processedAt = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);
        var paymentId = PaymentId.Create();
        var domainEvent = new PaymentProcessedDomainEvent(
            paymentId,
            OrderId.From(orderId),
            CustomerId.From(customerId),
            "grace@example.com",
            "Grace Hopper",
            Money.From(59.90m, Currency.Brl),
            Approved: true,
            processedAt
        );

        await handler.HandleAsync(domainEvent, TestContext.Current.CancellationToken);

        await publishEndpoint
            .Received(1)
            .Publish(
                Arg.Is<PaymentProcessedIntegrationEvent>(integrationEvent =>
                    integrationEvent.PaymentId == paymentId.Value
                    && integrationEvent.OrderId == orderId
                    && integrationEvent.CustomerId == customerId
                    && integrationEvent.CustomerEmail == "grace@example.com"
                    && integrationEvent.CustomerName == "Grace Hopper"
                    && integrationEvent.TotalAmount == 59.90m
                    && integrationEvent.Currency == "BRL"
                    && integrationEvent.IsApproved
                    && integrationEvent.ProcessedAt == processedAt
                ),
                Arg.Any<CancellationToken>()
            );
    }
}
