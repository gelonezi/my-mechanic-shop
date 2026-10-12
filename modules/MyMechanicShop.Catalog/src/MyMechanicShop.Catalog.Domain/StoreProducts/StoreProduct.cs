using System;
using MyMechanicShop.SharedKernel.ValueObjects;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace MyMechanicShop.Catalog.StoreProducts;

public sealed class StoreProduct : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; }

    public Guid ProductId { get; private set; }

    public MonetaryVo Price { get; private set; } = null!;

    public bool IsActive { get; private set; }

    private StoreProduct()
    {
    }

    internal StoreProduct(Guid id, Guid? tenantId, Guid productId, MonetaryVo price)
        : base(id)
    {
        TenantId = tenantId;
        ProductId = productId;
        Price = Check.NotNull(price, nameof(price));
        IsActive = true;
    }

    public StoreProduct SetPrice(MonetaryVo price)
    {
        Price = Check.NotNull(price, nameof(price));
        return this;
    }

    public StoreProduct Deactivate()
    {
        IsActive = false;
        return this;
    }

    internal StoreProduct Reactivate(MonetaryVo price)
    {
        SetPrice(price);
        IsActive = true;
        return this;
    }
}
