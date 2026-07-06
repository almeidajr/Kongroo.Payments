using System.ComponentModel.DataAnnotations;

namespace Kongroo.Payments.Infrastructure;

public sealed class PaymentApprovalOptions
{
    public const string SectionName = "Payments";

    [Range(0, double.MaxValue)]
    public decimal ApprovalLimit { get; init; }
}
