using System.ComponentModel.DataAnnotations;

namespace Kongroo.Payments.Infrastructure;

public sealed class PaymentApprovalOptions
{
    public const string SectionName = "Payments";

    /// <summary>Payments whose amount is at or below this limit are approved; above it, rejected.</summary>
    [Range(0, double.MaxValue)]
    public decimal ApprovalLimit { get; init; }
}
