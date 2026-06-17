using Kongroo.BuildingBlocks.Domain.Exceptions;
using Kongroo.Payments.Domain;
using Shouldly;

namespace Kongroo.Payments.UnitTests.Domain;

public sealed class PaymentTests
{
    private static readonly OrderId AnyOrder = OrderId.From(Guid.CreateVersion7());
    private static readonly CustomerId AnyCustomer = CustomerId.From(Guid.CreateVersion7());
    private static readonly DateTimeOffset ProcessedAt = new(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);

    private sealed class FixedPolicy(bool approved) : IPaymentApprovalPolicy
    {
        public bool IsApproved(Money total) => approved;
    }

    private static Payment NewPending() =>
        Payment.ForOrder(AnyOrder, AnyCustomer, "grace@example.com", "Grace Hopper", Money.From(100m, Currency.Brl));

    [Fact]
    public void ForOrder_CreatesPendingPayment()
    {
        var payment = NewPending();

        payment.Status.ShouldBe(PaymentStatus.Pending);
        payment.OrderId.ShouldBe(AnyOrder);
        payment.ProcessedAt.ShouldBeNull();
        payment.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void Process_WhenPolicyApproves_SetsApprovedAndRaisesEvent()
    {
        var payment = NewPending();

        payment.Process(new FixedPolicy(approved: true), ProcessedAt);

        payment.Status.ShouldBe(PaymentStatus.Approved);
        payment.ProcessedAt.ShouldBe(ProcessedAt);

        var raised = payment.DomainEvents.OfType<PaymentProcessedDomainEvent>().Single();
        raised.Approved.ShouldBeTrue();
        raised.OrderId.ShouldBe(AnyOrder);
        raised.CustomerId.ShouldBe(AnyCustomer);
        raised.Email.ShouldBe("grace@example.com");
        raised.CustomerName.ShouldBe("Grace Hopper");
        raised.ProcessedAt.ShouldBe(ProcessedAt);
    }

    [Fact]
    public void Process_WhenPolicyRejects_SetsRejectedAndRaisesEvent()
    {
        var payment = NewPending();

        payment.Process(new FixedPolicy(approved: false), ProcessedAt);

        payment.Status.ShouldBe(PaymentStatus.Rejected);
        payment.DomainEvents.OfType<PaymentProcessedDomainEvent>().Single().Approved.ShouldBeFalse();
    }

    [Fact]
    public void Process_WhenAlreadyProcessed_Throws()
    {
        var payment = NewPending();
        payment.Process(new FixedPolicy(approved: true), ProcessedAt);

        var act = () => payment.Process(new FixedPolicy(approved: true), ProcessedAt);

        Should.Throw<ConflictException>(act);
    }
}
