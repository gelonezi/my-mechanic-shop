using System;
using MyMechanicShop.SharedKernel.Enums;
using Shouldly;
using Xunit;

namespace MyMechanicShop.SharedKernel.ValueObjects;

public class MonetaryVo_Tests
{
    [Fact]
    public void Should_Hold_Amount_And_Currency()
    {
        var price = MonetaryVo.Create(49.90m, Currency.Brl);

        price.Amount.ShouldBe(49.90m);
        price.Currency.ShouldBe(Currency.Brl);
        price.ToString().ShouldBe("49.90 BRL");
    }

    [Fact]
    public void Should_Accept_Zero()
    {
        MonetaryVo.Create(0m, Currency.Brl).Amount.ShouldBe(0m);
    }

    [Fact]
    public void Should_Reject_A_Negative_Amount()
    {
        Should.Throw<ArgumentException>(() => MonetaryVo.Create(-0.01m, Currency.Brl));
    }

    [Theory]
    [InlineData(Currency.Undefined)]
    [InlineData((Currency)999)]
    public void Should_Reject_A_Currency_That_Is_Not_Accepted(Currency currency)
    {
        Should.Throw<ArgumentException>(() => MonetaryVo.Create(10m, currency));
    }

    [Fact]
    public void Should_Not_Round_The_Amount()
    {
        // A unit price per gram may need more places than the currency's decimals.
        MonetaryVo.Create(0.0375m, Currency.Brl).Amount.ShouldBe(0.0375m);
    }

    [Fact]
    public void Should_Compare_Amount_By_Value_Regardless_Of_Scale()
    {
        MonetaryVo.Create(12.50m, Currency.Brl)
            .ValueEquals(MonetaryVo.Create(12.5m, Currency.Brl))
            .ShouldBeTrue();
    }

    [Fact]
    public void Should_Not_Be_Equal_In_Another_Currency()
    {
        MonetaryVo.Create(12.50m, Currency.Brl)
            .ValueEquals(MonetaryVo.Create(12.50m, Currency.Usd))
            .ShouldBeFalse();
    }
}
