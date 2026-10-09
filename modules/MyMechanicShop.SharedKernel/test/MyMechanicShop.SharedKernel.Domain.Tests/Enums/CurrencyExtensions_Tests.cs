using System;
using Shouldly;
using Xunit;

namespace MyMechanicShop.SharedKernel.Enums;

public class CurrencyExtensions_Tests
{
    [Theory]
    [InlineData(Currency.Brl, 986, "BRL", "R$", 2)]
    [InlineData(Currency.Usd, 840, "USD", "US$", 2)]
    [InlineData(Currency.Eur, 978, "EUR", "€", 2)]
    public void Should_Expose_The_Iso_4217_Data(Currency currency, int numericCode, string isoCode, string symbol, int decimals)
    {
        ((int)currency).ShouldBe(numericCode);
        currency.GetIsoCode().ShouldBe(isoCode);
        currency.GetSymbol().ShouldBe(symbol);
        currency.GetDecimals().ShouldBe(decimals);
        currency.IsAccepted().ShouldBeTrue();
    }

    [Theory]
    [InlineData(Currency.Undefined)]
    [InlineData((Currency)999)]
    public void Should_Not_Accept_An_Unknown_Currency(Currency currency)
    {
        currency.IsAccepted().ShouldBeFalse();
        Should.Throw<ArgumentOutOfRangeException>(() => currency.GetIsoCode());
    }

    [Fact]
    public void Every_Currency_Except_Undefined_Should_Be_Accepted()
    {
        foreach (var currency in Enum.GetValues<Currency>())
        {
            currency.IsAccepted().ShouldBe(currency != Currency.Undefined, $"{currency} needs a row in CurrencyExtensions");
        }
    }
}
