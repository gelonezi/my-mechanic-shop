using System;
using MyMechanicShop.SharedKernel.ValueObjects;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace MyMechanicShop.Catalog.StoreProducts;

/// <summary>
/// A catalog <see cref="Products.Product"/> activated in a shop (tenant), with the price that shop
/// charges. At most one per shop and product; created through <see cref="StoreProductManager"/>.
/// </summary>
/// <remarks>
/// Not soft-deletable: a shop stops selling a product by deactivating it, which keeps the unique
/// (TenantId, ProductId) index simple. Stock is not here — it belongs to a future Inventory module.
/// </remarks>
public class StoreProduct : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; protected set; }

    /// <summary>The catalog product, referenced by id (another aggregate).</summary>
    public virtual Guid ProductId { get; protected set; }

    public virtual MonetaryVo Price { get; protected set; } = null!;

    public virtual bool IsActive { get; protected set; }

    protected StoreProduct()
    {
        // For EF Core.
    }

    internal StoreProduct(Guid id, Guid? tenantId, Guid productId, MonetaryVo price)
        : base(id)
    {
        TenantId = tenantId;
        ProductId = productId;
        SetPrice(price);
        IsActive = true;
    }

    public virtual StoreProduct SetPrice(MonetaryVo price)
    {
        Price = Check.NotNull(price, nameof(price));
        return this;
    }

    public virtual StoreProduct Deactivate()
    {
        IsActive = false;
        return this;
    }

    /// <summary>Sells a deactivated product again, at the given price.</summary>
    internal StoreProduct Reactivate(MonetaryVo price)
    {
        SetPrice(price);
        IsActive = true;
        return this;
    }
}
