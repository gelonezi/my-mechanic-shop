using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Values;

namespace MyMechanicShop.SharedKernel.ValueObjects;

/// <summary>A free-text description, trimmed, at most <see cref="DescriptionConsts.MaxLength"/> characters.</summary>
/// <remarks>
/// An optional description is a <c>null</c> <see cref="DescriptionVo"/> on the entity, never an
/// empty one. Compare with <see cref="ValueObject.ValueEquals"/>.
/// </remarks>
public sealed class DescriptionVo : ValueObject
{
    public string Value { get; private set; } = null!;

    private DescriptionVo()
    {
        // For EF Core.
    }

    private DescriptionVo(string value)
    {
        Value = value;
    }

    public static DescriptionVo Create(string value)
    {
        var trimmed = value?.Trim();
        Check.NotNullOrWhiteSpace(trimmed, nameof(value), DescriptionConsts.MaxLength);
        return new DescriptionVo(trimmed!);
    }

    public override string ToString() => Value;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
