using Kongroo.Payments.Domain;
using Kongroo.Payments.Infrastructure;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Kongroo.Payments.UnitTests.Infrastructure;

public sealed class ThresholdApprovalPolicyTests
{
    private static ThresholdApprovalPolicy PolicyWithLimit(decimal limit) =>
        new(Options.Create(new PaymentApprovalOptions { ApprovalLimit = limit }));

    [Theory]
    [InlineData(999.99, true)]
    [InlineData(1000, true)]
    [InlineData(1000.01, false)]
    public void IsApproved_WithConfiguredLimit_ShouldReturnApprovalDecision(decimal amount, bool expected) =>
        PolicyWithLimit(1000m).IsApproved(Money.From(amount, Currency.Brl)).ShouldBe(expected);
}
