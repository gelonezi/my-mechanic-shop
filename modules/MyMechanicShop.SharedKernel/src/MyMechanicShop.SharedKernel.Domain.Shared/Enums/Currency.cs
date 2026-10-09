namespace MyMechanicShop.SharedKernel.Enums;

/// <summary>
/// A currency the shop accepts. Values are the ISO 4217 <b>numeric</b> codes, so they are
/// standardized and never change; the alphabetic code, symbol and decimal places come from
/// <see cref="CurrencyExtensions"/>.
/// </summary>
/// <remarks>
/// The value is what EF stores, what the API sends and the key of the localized name
/// (<c>Enum:Currency.&lt;value&gt;</c>). To accept a new currency, add its ISO 4217 numeric
/// code here, its entry in <see cref="CurrencyExtensions"/> and its localized names.
/// </remarks>
public enum Currency
{
    /// <summary>Not set. The default value; never a valid currency for an amount.</summary>
    Undefined = 0,

    /// <summary>Brazilian real (BRL), the shop's default currency.</summary>
    Brl = 986,

    /// <summary>United States dollar (USD).</summary>
    Usd = 840,

    /// <summary>Euro (EUR).</summary>
    Eur = 978,
}
