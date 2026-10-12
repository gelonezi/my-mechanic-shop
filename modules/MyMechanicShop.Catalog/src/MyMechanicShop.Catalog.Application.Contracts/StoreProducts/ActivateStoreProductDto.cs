using System;

namespace MyMechanicShop.Catalog.StoreProducts;

public class ActivateStoreProductDto : StorePriceDto
{
    public Guid ProductId { get; set; }
}
