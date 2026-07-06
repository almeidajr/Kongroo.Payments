using Kongroo.BuildingBlocks.Application;
using Kongroo.Payments.Domain;
using Kongroo.Payments.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Kongroo.Payments.Application;

public sealed class ProcessPaymentCommandHandler(
    PaymentsDbContext context,
    IPaymentApprovalPolicy policy,
    TimeProvider timeProvider,
    IUnitOfWork unitOfWork
)
{
    public async Task HandleAsync(ProcessPaymentCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var orderId = OrderId.From(command.OrderId);

        var alreadyProcessed = await context.Payments.AnyAsync(
            payment => payment.OrderId == orderId,
            cancellationToken
        );
        if (alreadyProcessed)
        {
            return;
        }

        var payment = Payment.ForOrder(
            orderId,
            CustomerId.From(command.CustomerId),
            command.Email,
            command.CustomerName,
            Money.From(command.Amount, CurrencyMappings.FromCode(command.Currency))
        );

        payment.Process(policy, timeProvider.GetUtcNow());

        context.Payments.Add(payment);
        await unitOfWork.CommitAsync(cancellationToken);
    }
}
