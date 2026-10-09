using System;
using Shouldly;
using Xunit;

namespace MyMechanicShop.SharedKernel.ValueObjects;

public class NameVo_Tests
{
    [Fact]
    public void Should_Trim_The_Value()
    {
        NameVo.Create("  Oil filter  ").Value.ShouldBe("Oil filter");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Reject_A_Blank_Name(string? value)
    {
        Should.Throw<ArgumentException>(() => NameVo.Create(value!));
    }

    [Fact]
    public void Should_Accept_Exactly_MaxLength_And_Reject_One_More()
    {
        NameVo.Create(new string('x', NameConsts.MaxLength)).Value.Length.ShouldBe(NameConsts.MaxLength);
        Should.Throw<ArgumentException>(() => NameVo.Create(new string('x', NameConsts.MaxLength + 1)));
    }

    [Fact]
    public void Should_Measure_MaxLength_After_Trimming()
    {
        var padded = "  " + new string('x', NameConsts.MaxLength) + "  ";

        NameVo.Create(padded).Value.Length.ShouldBe(NameConsts.MaxLength);
    }

    [Fact]
    public void Should_Compare_By_Value_Only_Through_ValueEquals()
    {
        var a = NameVo.Create("Spark plug");
        var b = NameVo.Create("Spark plug");

        a.ValueEquals(b).ShouldBeTrue();
        a.ValueEquals(NameVo.Create("Bulb")).ShouldBeFalse();

        // ABP's ValueObject does not override Equals/==: they stay reference comparisons.
        (a == b).ShouldBeFalse();
    }
}
