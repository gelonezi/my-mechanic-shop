using System;
using System.Collections.Generic;

namespace MyMechanicShop.SharedKernel.Enums;

/// <summary>
/// Static ISO 4217 data for each <see cref="Currency"/>. Kept in code rather than taken from the
/// OS (<c>RegionInfo</c>/ICU), whose data varies by platform and is absent in invariant-globalization
/// containers. The display <b>name</b> is not here: it is localized (<c>Enum:Currency.&lt;value&gt;</c>).
/// </summary>
public static class CurrencyExtensions
{
    private sealed record CurrencyInfo(string IsoCode, string Symbol, int Decimals);

    // Symbols are the unambiguous forms used in Brazil ("US$", not "$"). Locale-aware
    // formatting of an amount stays with the UI (Angular's currency pipe, from the ISO code).
    private static readonly Dictionary<Currency, CurrencyInfo> Infos = new()
    {
        [Currency.Brl] = new("BRL", "R$", 2),
        [Currency.Usd] = new("USD", "US$", 2),
        [Currency.Eur] = new("EUR", "€", 2),
    };

    /// <summary>The ISO 4217 alphabetic code: <c>BRL</c>, <c>USD</c>, <c>EUR</c>.</summary>
    public static string GetIsoCode(this Currency currency) => GetInfo(currency).IsoCode;

    /// <summary>The symbol: <c>R$</c>, <c>US$</c>, <c>€</c>.</summary>
    public static string GetSymbol(this Currency currency) => GetInfo(currency).Symbol;

    /// <summary>The ISO 4217 minor unit: how many decimal places amounts are shown and rounded to.</summary>
    public static int GetDecimals(this Currency currency) => GetInfo(currency).Decimals;

    /// <summary>Whether this is an accepted currency (defined, and not <see cref="Currency.Undefined"/>).</summary>
    public static bool IsAccepted(this Currency currency) => Infos.ContainsKey(currency);

    private static CurrencyInfo GetInfo(Currency currency)
    {
        return Infos.TryGetValue(currency, out var info)
            ? info
            : throw new ArgumentOutOfRangeException(nameof(currency), currency, "Not an accepted currency.");
    }
}
