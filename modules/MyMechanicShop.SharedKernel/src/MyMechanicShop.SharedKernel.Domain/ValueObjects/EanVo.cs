using System;
using System.Collections.Generic;
using MyMechanicShop.SharedKernel.Enums;
using Volo.Abp;
using Volo.Abp.Domain.Values;

namespace MyMechanicShop.SharedKernel.ValueObjects;

/// <summary>
/// A GS1 EAN-8 or EAN-13 barcode: digits only, with a valid check digit.
/// </summary>
/// <remarks>
/// Only <see cref="Value"/> is stored; <see cref="Format"/> follows from its length.
/// User input should be checked with <see cref="IsValid"/> in the DTO, so a bad code is a 400;
/// <see cref="Create"/> throws <see cref="ArgumentException"/>, a domain invariant.
/// Compare with <see cref="ValueObject.ValueEquals"/>.
/// </remarks>
public sealed class EanVo : ValueObject
{
    public string Value { get; private set; } = null!;

    public EanFormat Format => Value.Length == EanConsts.Ean8Length ? EanFormat.Ean8 : EanFormat.Ean13;

    private EanVo()
    {
        // For EF Core.
    }

    private EanVo(string value)
    {
        Value = value;
    }

    public static EanVo Create(string value)
    {
        var trimmed = Check.NotNullOrWhiteSpace(value, nameof(value)).Trim();

        if (!IsValid(trimmed))
        {
            throw new ArgumentException(
                $"'{value}' is not a valid EAN-8 or EAN-13: {EanConsts.Ean8Length} or {EanConsts.Ean13Length} digits with a valid check digit.",
                nameof(value));
        }

        return new EanVo(trimmed);
    }

    /// <inheritdoc cref="EanValidator.IsValid"/>
    public static bool IsValid(string? value) => EanValidator.IsValid(value);

    public override string ToString() => Value;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
