using Kongroo.Payments.Domain;
using Kongroo.Payments.Infrastructure;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Kongroo.Payments.UnitTests.Infrastructure;

public sealed class ThresholdApprovalPolicyTests
{
    private static ThresholdApprovalPolicy PolicyWithLimit(decimal limit) =>
        new(Options.Create(new PaymentApprovalOptions { ApprovalLimit = limit }));

    [Fact]
    public void IsApproved_WhenAmountBelowLimit_ReturnsTrue() =>
        PolicyWithLimit(1000m).IsApproved(Money.From(999.99m, Currency.Brl)).ShouldBeTrue();

    [Fact]
    public void IsApproved_WhenAmountEqualsLimit_ReturnsTrue() =>
        PolicyWithLimit(1000m).IsApproved(Money.From(1000m, Currency.Brl)).ShouldBeTrue();

    [Fact]
    public void IsApproved_WhenAmountAboveLimit_ReturnsFalse() =>
        PolicyWithLimit(1000m).IsApproved(Money.From(1000.01m, Currency.Brl)).ShouldBeFalse();
}
