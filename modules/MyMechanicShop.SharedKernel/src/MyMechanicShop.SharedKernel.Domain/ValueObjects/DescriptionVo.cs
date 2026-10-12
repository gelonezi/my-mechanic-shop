using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Values;

namespace MyMechanicShop.SharedKernel.ValueObjects;

public sealed class DescriptionVo : ValueObject
{
    public string Value { get; private set; } = null!;

    private DescriptionVo()
    {
    }

    private DescriptionVo(string value)
    {
        Value = value;
    }

    public static DescriptionVo Create(string value)
    {
        var trimmed = value?.Trim();
        Check.NotNullOrWhiteSpace(trimmed, nameof(value), DescriptionConsts.MaxLength);
        return new DescriptionVo(trimmed);
    }

    public override string ToString() => Value;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
