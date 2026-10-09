namespace MyMechanicShop.SharedKernel.Enums;

/// <summary>
/// Unit in which a service (labor, towing) is charged. Products use <see cref="ProductUnit"/>.
/// </summary>
/// <remarks>
/// Values are explicit and grouped in ranges of ten (per job, time, distance), leaving room to
/// add a unit next to its relatives. EF stores the value as an int and the API sends it as a
/// number — it is also the key of the localized text (<c>Enum:ServiceUnit.&lt;value&gt;</c>).
/// Never renumber or reuse a value.
/// </remarks>
public enum ServiceUnit
{
    /// <summary>Not set. The default value; never a valid unit for a service.</summary>
    Undefined = 0,

    // Per job (1-9)

    /// <summary>The whole service at a fixed price: oil change, wheel alignment, inspection.</summary>
    FlatRate = 1,

    // Time (10-19)

    /// <summary>Hour: labor charged by time.</summary>
    Hour = 10,

    // Distance (20-29)

    /// <summary>Kilometer: towing charged by distance.</summary>
    Kilometer = 20,
}
