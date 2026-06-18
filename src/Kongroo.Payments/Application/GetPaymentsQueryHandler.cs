using Kongroo.Payments.Domain;
using Kongroo.Payments.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Kongroo.Payments.Application;

public sealed class GetPaymentsQueryHandler(PaymentsDbContext context)
{
    public async Task<IReadOnlyList<PaymentResponse>> HandleAsync(
        GetPaymentsQuery query,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        var customerId = CustomerId.From(query.CustomerId);

        var payments = await context
            .Payments.Where(payment => payment.CustomerId == customerId)
            .OrderByDescending(payment => payment.ProcessedAt)
            .ToListAsync(cancellationToken);

        return [.. payments.Select(PaymentMapping.ToResponse)];
    }
}
