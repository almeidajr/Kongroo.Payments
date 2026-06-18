using Kongroo.BuildingBlocks.Domain.Exceptions;
using Kongroo.Payments.Domain;
using Kongroo.Payments.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Kongroo.Payments.Application;

public sealed class GetPaymentQueryHandler(PaymentsDbContext context)
{
    public async Task<PaymentResponse> HandleAsync(GetPaymentQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var orderId = OrderId.From(query.OrderId);

        var payment =
            await context.Payments.SingleOrDefaultAsync(candidate => candidate.OrderId == orderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Payment), $"order '{query.OrderId}'");

        // Non-admins may only see their own payments; report "not found" rather than leak existence.
        if (!query.CallerIsAdmin && payment.CustomerId != CustomerId.From(query.CallerId))
        {
            throw new NotFoundException(nameof(Payment), $"order '{query.OrderId}'");
        }

        return PaymentMapping.ToResponse(payment);
    }
}
