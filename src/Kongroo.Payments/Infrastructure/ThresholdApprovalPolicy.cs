using Kongroo.Payments.Domain;
using Microsoft.Extensions.Options;

namespace Kongroo.Payments.Infrastructure;

public sealed class ThresholdApprovalPolicy(IOptions<PaymentApprovalOptions> options) : IPaymentApprovalPolicy
{
    public bool IsApproved(Money total)
    {
        ArgumentNullException.ThrowIfNull(total);

        return total.Amount <= options.Value.ApprovalLimit;
    }
}
