namespace MyMechanicShop.SharedKernel.Enums;

/// <summary>
/// Unit in which a product (a part, fluid or material) is sold. Services use <see cref="ServiceUnit"/>.
/// </summary>
/// <remarks>
/// Values are explicit and grouped by quantity in ranges of ten (count, mass, volume, length,
/// area), leaving room to add a unit next to its relatives. EF stores the value as an int and
/// the API sends it as a number — it is also the key of the localized text
/// (<c>Enum:ProductUnit.&lt;value&gt;</c>). Never renumber or reuse a value.
/// </remarks>
public enum ProductUnit
{
    /// <summary>Not set. The default value; never a valid unit for a product.</summary>
    Undefined = 0,

    // Count (1-9)

    /// <summary>A single piece: filter, spark plug, bulb, battery.</summary>
    Unit = 1,

    /// <summary>Two matching pieces sold together: wiper blades, headlamp bulbs.</summary>
    Pair = 2,

    /// <summary>A set of different parts sold together: brake pads, timing belt or clutch kit.</summary>
    Kit = 3,

    /// <summary>A box of small parts: fuses, clips, screws.</summary>
    Box = 4,

    /// <summary>A package or container sold as a whole.</summary>
    Package = 5,

    // Mass (10-19)

    /// <summary>Gram.</summary>
    Gram = 10,

    /// <summary>Kilogram: grease, bulk materials.</summary>
    Kilogram = 11,

    // Volume (20-29)

    /// <summary>Milliliter: additives, small fluid bottles.</summary>
    Milliliter = 20,

    /// <summary>Liter: engine oil, coolant, brake and transmission fluids.</summary>
    Liter = 21,

    /// <summary>Cubic meter: natural gas (CNG).</summary>
    CubicMeter = 22,

    // Length (30-39)

    /// <summary>Centimeter.</summary>
    Centimeter = 30,

    /// <summary>Meter: hoses, electrical wire, cables.</summary>
    Meter = 31,

    // Area (40-49)

    /// <summary>Square meter: upholstery fabric, window film, wrap vinyl.</summary>
    SquareMeter = 40,
}
