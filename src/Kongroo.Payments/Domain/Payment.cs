using Kongroo.BuildingBlocks.Domain;
using Kongroo.BuildingBlocks.Domain.Exceptions;

namespace Kongroo.Payments.Domain;

public sealed class Payment : Entity<PaymentId>
{
    private Payment() { }

    public OrderId OrderId { get; private set; } = null!;

    public CustomerId CustomerId { get; private set; } = null!;

    public string Email { get; private set; } = null!;

    public string CustomerName { get; private set; } = null!;

    public Money Total { get; private set; } = null!;

    public PaymentStatus Status { get; private set; }

    public DateTimeOffset? ProcessedAt { get; private set; }

    public static Payment ForOrder(
        OrderId orderId,
        CustomerId customerId,
        string email,
        string customerName,
        Money total
    )
    {
        ArgumentNullException.ThrowIfNull(orderId);
        ArgumentNullException.ThrowIfNull(customerId);
        ArgumentNullException.ThrowIfNull(total);

        return new Payment
        {
            Id = PaymentId.Create(),
            OrderId = orderId,
            CustomerId = customerId,
            Email = email,
            CustomerName = customerName,
            Total = total,
            Status = PaymentStatus.Pending,
        };
    }

    public void Process(IPaymentApprovalPolicy policy, DateTimeOffset processedAt)
    {
        ArgumentNullException.ThrowIfNull(policy);

        if (Status != PaymentStatus.Pending)
        {
            throw new ConflictException(nameof(Payment), $"payment is already '{Status}'");
        }

        var approved = policy.IsApproved(Total);
        Status = approved ? PaymentStatus.Approved : PaymentStatus.Rejected;
        ProcessedAt = processedAt;

        RaiseDomainEvent(
            new PaymentProcessedDomainEvent(Id, OrderId, CustomerId, Email, CustomerName, Total, approved, processedAt)
        );
    }
}
