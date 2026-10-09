using System;
using Shouldly;
using Xunit;

namespace MyMechanicShop.SharedKernel.ValueObjects;

public class DescriptionVo_Tests
{
    [Fact]
    public void Should_Trim_The_Value()
    {
        DescriptionVo.Create("  Synthetic 5W-30 engine oil  ").Value.ShouldBe("Synthetic 5W-30 engine oil");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Reject_A_Blank_Description(string? value)
    {
        Should.Throw<ArgumentException>(() => DescriptionVo.Create(value!));
    }

    [Fact]
    public void Should_Accept_Exactly_MaxLength_And_Reject_One_More()
    {
        DescriptionVo.Create(new string('x', DescriptionConsts.MaxLength)).Value.Length.ShouldBe(DescriptionConsts.MaxLength);
        Should.Throw<ArgumentException>(() => DescriptionVo.Create(new string('x', DescriptionConsts.MaxLength + 1)));
    }
}
