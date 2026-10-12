using System;
using System.Collections.Generic;
using MyMechanicShop.SharedKernel.Enums;
using Volo.Abp;
using Volo.Abp.Domain.Values;

namespace MyMechanicShop.SharedKernel.ValueObjects;

public sealed class EanVo : ValueObject
{
    public string Value { get; private set; } = null!;

    public EanFormat Format => Value.Length == EanConsts.Ean8Length ? EanFormat.Ean8 : EanFormat.Ean13;

    private EanVo()
    {
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

    public static bool IsValid(string? value) => EanValidator.IsValid(value);

    public override string ToString() => Value;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
