using Kongroo.BuildingBlocks.Contracts;
using Kongroo.Payments.Application;
using Kongroo.Payments.Domain;
using MassTransit;
using NSubstitute;

namespace Kongroo.Payments.UnitTests.Application;

public sealed class PaymentProcessedDomainEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_PublishesIntegrationEventMappedFromDomainEvent()
    {
        var publishEndpoint = Substitute.For<IPublishEndpoint>();
        var handler = new PaymentProcessedDomainEventHandler(publishEndpoint);

        var orderId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();
        var processedAt = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);
        var domainEvent = new PaymentProcessedDomainEvent(
            PaymentId.Create(),
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
                    integrationEvent.OrderId == orderId
                    && integrationEvent.UserId == customerId
                    && integrationEvent.Email == "grace@example.com"
                    && integrationEvent.CustomerName == "Grace Hopper"
                    && integrationEvent.Amount == 59.90m
                    && integrationEvent.Currency == "BRL"
                    && integrationEvent.Approved
                    && integrationEvent.ProcessedAt == processedAt
                ),
                Arg.Any<CancellationToken>()
            );
    }
}
