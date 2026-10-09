using System;
using System.Collections.Generic;
using System.Globalization;
using MyMechanicShop.SharedKernel.Enums;
using Volo.Abp;
using Volo.Abp.Domain.Values;

namespace MyMechanicShop.SharedKernel.ValueObjects;

/// <summary>An amount of money in an accepted <see cref="Enums.Currency"/>. Never negative.</summary>
/// <remarks>
/// <see cref="Amount"/> is <c>decimal</c>, never <c>float</c>/<c>double</c>: binary floating point
/// cannot represent most decimal fractions exactly (0.1 + 0.2 != 0.3), so totals drift.
/// The amount is not rounded here (a unit price per gram may need more places than the
/// currency's <see cref="CurrencyExtensions.GetDecimals"/>); round totals when presenting or
/// charging them. Compare with <see cref="ValueObject.ValueEquals"/>.
/// </remarks>
public sealed class MonetaryVo : ValueObject
{
    public decimal Amount { get; private set; }

    public Currency Currency { get; private set; }

    private MonetaryVo()
    {
        // For EF Core.
    }

    private MonetaryVo(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static MonetaryVo Create(decimal amount, Currency currency)
    {
        if (!currency.IsAccepted())
        {
            throw new ArgumentException($"{currency} is not an accepted currency.", nameof(currency));
        }

        Check.Range(amount, nameof(amount), 0m, decimal.MaxValue);
        return new MonetaryVo(amount, currency);
    }

    public override string ToString() => $"{Amount.ToString(CultureInfo.InvariantCulture)} {Currency.GetIsoCode()}";

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Amount;
        yield return Currency;
    }
}
