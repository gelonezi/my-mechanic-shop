using System;
using MyMechanicShop.SharedKernel.Enums;
using MyMechanicShop.SharedKernel.ValueObjects;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace MyMechanicShop.Catalog.Products;

public sealed class Product : FullAuditedAggregateRoot<Guid>
{
    public NameVo Name { get; private set; } = null!;

    public NameVo? Brand { get; private set; }

    public EanVo? Ean { get; private set; }

    public DescriptionVo? Description { get; private set; }

    public ProductUnit Unit { get; private set; }

    private Product()
    {
    }

    public Product(Guid id, NameVo name, ProductUnit unit)
        : base(id)
    {
        Name = Check.NotNull(name, nameof(name));
        Unit = ValidateUnit(unit);
    }

    public Product SetName(NameVo name)
    {
        Name = Check.NotNull(name, nameof(name));
        return this;
    }

    public Product SetBrand(NameVo? brand)
    {
        Brand = brand;
        return this;
    }

    public Product SetEan(EanVo? ean)
    {
        Ean = ean;
        return this;
    }

    public Product SetDescription(DescriptionVo? description)
    {
        Description = description;
        return this;
    }

    public Product SetUnit(ProductUnit unit)
    {
        Unit = ValidateUnit(unit);
        return this;
    }

    private static ProductUnit ValidateUnit(ProductUnit unit)
    {
        if (unit == ProductUnit.Undefined || !Enum.IsDefined(unit))
        {
            throw new ArgumentException($"{unit} is not a valid product unit.", nameof(unit));
        }

        return unit;
    }
}
