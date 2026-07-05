using Kongroo.Catalog.Contracts;
using MassTransit;

namespace Kongroo.Payments.Application;

/// <summary>Processes a payment when an order is placed.</summary>
public sealed class OrderPlacedIntegrationEventConsumer(ProcessPaymentCommandHandler handler)
    : IConsumer<OrderPlacedIntegrationEvent>
{
    public Task Consume(ConsumeContext<OrderPlacedIntegrationEvent> context)
    {
        var message = context.Message;

        return handler.HandleAsync(
            new ProcessPaymentCommand(
                message.OrderId,
                message.CustomerId,
                message.Email,
                message.CustomerName,
                message.Amount,
                message.Currency
            ),
            context.CancellationToken
        );
    }
}
