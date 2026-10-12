using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Values;

namespace MyMechanicShop.SharedKernel.ValueObjects;

public sealed class NameVo : ValueObject
{
    public string Value { get; private set; } = null!;

    private NameVo()
    {
    }

    private NameVo(string value)
    {
        Value = value;
    }

    public static NameVo Create(string value)
    {
        var trimmed = value?.Trim();
        Check.NotNullOrWhiteSpace(trimmed, nameof(value), NameConsts.MaxLength);
        return new NameVo(trimmed);
    }

    public override string ToString() => Value;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
