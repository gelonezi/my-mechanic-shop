using System;
using System.Collections.Generic;
using System.Linq;
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

    /// <summary>
    /// Whether <paramref name="value"/> is an EAN-8 or EAN-13: only digits, the right length, and a
    /// check digit matching the GS1 mod-10 algorithm.
    /// </summary>
    public static bool IsValid(string? value)
    {
        return value is { Length: EanConsts.Ean8Length or EanConsts.Ean13Length }
               && value.All(char.IsAsciiDigit)
               && value[^1] - '0' == CalculateCheckDigit(value[..^1]);
    }

    /// <summary>
    /// GS1 mod-10: from the rightmost payload digit leftwards, weights alternate 3, 1, 3, …;
    /// the check digit brings the weighted sum up to a multiple of ten.
    /// </summary>
    private static int CalculateCheckDigit(string payload)
    {
        var sum = 0;
        for (var i = 0; i < payload.Length; i++)
        {
            var digit = payload[payload.Length - 1 - i] - '0';
            sum += i % 2 == 0 ? digit * 3 : digit;
        }

        return (10 - sum % 10) % 10;
    }

    public override string ToString() => Value;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
