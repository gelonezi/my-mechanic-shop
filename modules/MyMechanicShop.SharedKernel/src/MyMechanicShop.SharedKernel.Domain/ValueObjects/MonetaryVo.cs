using System;
using System.Collections.Generic;
using System.Globalization;
using MyMechanicShop.SharedKernel.Enums;
using Volo.Abp;
using Volo.Abp.Domain.Values;

namespace MyMechanicShop.SharedKernel.ValueObjects;

public sealed class MonetaryVo : ValueObject
{
    public decimal Amount { get; private set; }

    public Currency Currency { get; private set; }

    private MonetaryVo()
    {
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

        Check.Range(amount, nameof(amount), 0m);
        return new MonetaryVo(amount, currency);
    }

    public override string ToString() => $"{Amount.ToString(CultureInfo.InvariantCulture)} {Currency.GetIsoCode()}";

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Amount;
        yield return Currency;
    }
}
