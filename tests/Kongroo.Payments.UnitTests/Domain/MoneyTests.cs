using Kongroo.Payments.Domain;
using Shouldly;

namespace Kongroo.Payments.UnitTests.Domain;

public sealed class MoneyTests
{
    [Fact]
    public void From_WithNonNegativeAmount_ShouldCreateMoney()
    {
        var money = Money.From(59.90m, Currency.Brl);

        money.Amount.ShouldBe(59.90m);
        money.Currency.ShouldBe(Currency.Brl);
    }

    [Fact]
    public void From_WithNegativeAmount_ShouldThrowArgumentOutOfRangeException()
    {
        var act = () => Money.From(-1m, Currency.Usd);

        Should.Throw<ArgumentOutOfRangeException>(act);
    }
}
