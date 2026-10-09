using System;
using MyMechanicShop.SharedKernel.Enums;
using MyMechanicShop.SharedKernel.ValueObjects;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace MyMechanicShop.Catalog.Products;

/// <summary>
/// A product of the global catalog, shared by every shop (host data: no tenant) and maintained by
/// the system administrator only. A shop sells it through a <see cref="StoreProducts.StoreProduct"/>,
/// which holds the shop's own price.
/// </summary>
/// <remarks>
/// Free-form attributes go in ABP's <c>ExtraProperties</c>, inherited from the aggregate root.
/// Members are virtual with protected setters, as in ABP's own modules, so a consuming application
/// can extend the entity.
/// </remarks>
public class Product : FullAuditedAggregateRoot<Guid>
{
    public virtual NameVo Name { get; protected set; } = null!;

    public virtual NameVo? Brand { get; protected set; }

    public virtual EanVo? Ean { get; protected set; }

    public virtual DescriptionVo? Description { get; protected set; }

    public virtual ProductUnit Unit { get; protected set; }

    protected Product()
    {
        // For EF Core.
    }

    public Product(Guid id, NameVo name, ProductUnit unit)
        : base(id)
    {
        SetName(name);
        SetUnit(unit);
    }

    public virtual Product SetName(NameVo name)
    {
        Name = Check.NotNull(name, nameof(name));
        return this;
    }

    public virtual Product SetBrand(NameVo? brand)
    {
        Brand = brand;
        return this;
    }

    public virtual Product SetEan(EanVo? ean)
    {
        Ean = ean;
        return this;
    }

    public virtual Product SetDescription(DescriptionVo? description)
    {
        Description = description;
        return this;
    }

    public virtual Product SetUnit(ProductUnit unit)
    {
        if (unit == ProductUnit.Undefined || !Enum.IsDefined(unit))
        {
            throw new ArgumentException($"{unit} is not a valid product unit.", nameof(unit));
        }

        Unit = unit;
        return this;
    }
}
