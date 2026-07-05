using Kongroo.Payments.Domain;
using Shouldly;

namespace Kongroo.Payments.UnitTests.Domain;

public sealed class CurrencyMappingsTests
{
    [Theory]
    [InlineData(Currency.Brl, "BRL")]
    [InlineData(Currency.Eur, "EUR")]
    [InlineData(Currency.Usd, "USD")]
    public void ToCode_WithDefinedCurrency_ShouldReturnIsoCode(Currency currency, string expected) =>
        CurrencyMappings.ToCode(currency).ShouldBe(expected);

    [Theory]
    [InlineData("BRL", Currency.Brl)]
    [InlineData("EUR", Currency.Eur)]
    [InlineData("USD", Currency.Usd)]
    public void FromCode_WithIsoCode_ShouldReturnCurrency(string code, Currency expected) =>
        CurrencyMappings.FromCode(code).ShouldBe(expected);
}
