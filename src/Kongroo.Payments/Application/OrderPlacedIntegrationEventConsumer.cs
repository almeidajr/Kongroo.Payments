using Kongroo.Catalog.Contracts;
using MassTransit;

namespace Kongroo.Payments.Application;

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
                message.CustomerEmail,
                message.CustomerName,
                message.TotalAmount,
                message.Currency
            ),
            context.CancellationToken
        );
    }
}
