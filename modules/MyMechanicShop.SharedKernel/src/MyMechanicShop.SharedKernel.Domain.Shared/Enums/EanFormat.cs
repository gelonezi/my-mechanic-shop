namespace MyMechanicShop.SharedKernel.Enums;

/// <summary>
/// The GS1 barcode format of an EAN. Each value is the format's number of digits.
/// Localized as <c>Enum:EanFormat.&lt;value&gt;</c>.
/// </summary>
public enum EanFormat
{
    /// <summary>Not set. The default value; never the format of a valid EAN.</summary>
    Undefined = 0,

    /// <summary>EAN-8: small packages where an EAN-13 does not fit.</summary>
    Ean8 = 8,

    /// <summary>EAN-13: the standard retail barcode (Brazilian products start with 789/790).</summary>
    Ean13 = 13,
}
