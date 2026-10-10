using System.Linq;

namespace MyMechanicShop.SharedKernel.ValueObjects;

/// <summary>
/// GS1 EAN-8 / EAN-13 validation, in Domain.Shared so DTOs (Application.Contracts) can reject a bad
/// barcode with a 400 before it reaches <c>EanVo</c>, which uses the same rule.
/// </summary>
public static class EanValidator
{
    /// <summary>
    /// Whether <paramref name="value"/> is an EAN-8 or EAN-13: only ASCII digits, the right length, and
    /// a check digit matching the GS1 mod-10 algorithm.
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
}
