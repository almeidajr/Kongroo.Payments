namespace Kongroo.Payments.Domain;

/// <summary>Decides whether a payment of a given total is approved.</summary>
public interface IPaymentApprovalPolicy
{
    bool IsApproved(Money total);
}
