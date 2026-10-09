using System;
using MyMechanicShop.SharedKernel.Enums;
using Shouldly;
using Xunit;

namespace MyMechanicShop.SharedKernel.ValueObjects;

public class EanVo_Tests
{
    // GS1's published examples.
    private const string ValidEan13 = "4006381333931";
    private const string ValidEan8 = "96385074";

    [Theory]
    [InlineData(ValidEan13, EanFormat.Ean13)]
    [InlineData(ValidEan8, EanFormat.Ean8)]
    public void Should_Accept_A_Valid_Code_And_Derive_Its_Format(string code, EanFormat format)
    {
        var ean = EanVo.Create(code);

        ean.Value.ShouldBe(code);
        ean.Format.ShouldBe(format);
    }

    [Fact]
    public void Should_Trim_The_Value()
    {
        EanVo.Create($"  {ValidEan8}  ").Value.ShouldBe(ValidEan8);
    }

    [Theory]
    [InlineData("4006381333932")]     // EAN-13, wrong check digit
    [InlineData("96385075")]          // EAN-8, wrong check digit
    [InlineData("123456789012")]      // 12 digits
    [InlineData("12345678901234")]    // 14 digits
    [InlineData("40063813339A1")]     // a letter
    [InlineData("4006381 333931")]    // inner space
    [InlineData("０１２３４５６５")]  // full-width digits: char.IsDigit accepts them, IsAsciiDigit does not
    [InlineData("")]
    [InlineData(null)]
    public void IsValid_Should_Reject(string? code)
    {
        EanVo.IsValid(code).ShouldBeFalse();
    }

    [Fact]
    public void Create_Should_Throw_For_An_Invalid_Code()
    {
        Should.Throw<ArgumentException>(() => EanVo.Create("4006381333932"));
    }

    [Fact]
    public void Should_Compare_By_Value_Through_ValueEquals()
    {
        EanVo.Create(ValidEan13).ValueEquals(EanVo.Create(ValidEan13)).ShouldBeTrue();
        EanVo.Create(ValidEan13).ValueEquals(EanVo.Create(ValidEan8)).ShouldBeFalse();
    }
}
