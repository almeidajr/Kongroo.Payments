using Kongroo.BuildingBlocks.Application;
using Kongroo.Payments.Contracts;
using Kongroo.Payments.Domain;
using MassTransit;

namespace Kongroo.Payments.Application;

public sealed class PaymentProcessedDomainEventHandler(IPublishEndpoint publishEndpoint)
    : DomainEventHandler<PaymentProcessedDomainEvent>
{
    protected override async Task HandleAsync(
        PaymentProcessedDomainEvent domainEvent,
        CancellationToken cancellationToken
    ) =>
        await publishEndpoint.Publish(
            new PaymentProcessedIntegrationEvent(
                domainEvent.PaymentId.Value,
                domainEvent.OrderId.Value,
                domainEvent.CustomerId.Value,
                domainEvent.Email,
                domainEvent.CustomerName,
                domainEvent.Total.Amount,
                CurrencyMappings.ToCode(domainEvent.Total.Currency),
                domainEvent.Approved,
                domainEvent.ProcessedAt
            ),
            cancellationToken
        );
}
