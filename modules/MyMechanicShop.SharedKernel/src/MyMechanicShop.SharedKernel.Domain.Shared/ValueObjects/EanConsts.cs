namespace MyMechanicShop.SharedKernel.ValueObjects;

public static class EanConsts
{
    public const int Ean8Length = 8;

    public const int Ean13Length = 13;

    /// <summary>For DTO <c>[StringLength]</c> and the database column.</summary>
    public const int MaxLength = Ean13Length;
}
