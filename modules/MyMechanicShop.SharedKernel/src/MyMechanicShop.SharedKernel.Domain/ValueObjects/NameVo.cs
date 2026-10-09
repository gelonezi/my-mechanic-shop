using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Values;

namespace MyMechanicShop.SharedKernel.ValueObjects;

/// <summary>A required name, trimmed, at most <see cref="NameConsts.MaxLength"/> characters.</summary>
/// <remarks>
/// ABP's <see cref="ValueObject"/> compares by value through <see cref="ValueObject.ValueEquals"/>;
/// <c>==</c> and <c>Equals</c> stay reference comparisons.
/// </remarks>
public sealed class NameVo : ValueObject
{
    public string Value { get; private set; } = null!;

    private NameVo()
    {
        // For EF Core.
    }

    private NameVo(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Domain invariant, guarded with ABP's <see cref="Check"/> (an <see cref="System.ArgumentException"/>).
    /// User input is validated earlier, by the DTO's <c>[Required]</c> / <c>[StringLength]</c>.
    /// </summary>
    public static NameVo Create(string value)
    {
        var trimmed = value?.Trim();
        Check.NotNullOrWhiteSpace(trimmed, nameof(value), NameConsts.MaxLength);
        return new NameVo(trimmed!);
    }

    public override string ToString() => Value;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
