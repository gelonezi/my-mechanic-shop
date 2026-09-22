using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace MyMechanicShop.Catalog.Products;

public class Product : AuditedAggregateRoot<Guid>
{
    public string Name { get; set; }
    public float Price { get; set; }
    public int StockCount { get; set; }
}