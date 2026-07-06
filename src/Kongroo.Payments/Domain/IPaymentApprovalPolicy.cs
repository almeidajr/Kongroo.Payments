namespace Kongroo.Payments.Domain;

public interface IPaymentApprovalPolicy
{
    bool IsApproved(Money total);
}
