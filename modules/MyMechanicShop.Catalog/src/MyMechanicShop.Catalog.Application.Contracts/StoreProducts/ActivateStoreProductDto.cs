using System;

namespace MyMechanicShop.Catalog.StoreProducts;

public class ActivateStoreProductDto : StorePriceDto
{
    /// <summary>The catalog product to start selling.</summary>
    public Guid ProductId { get; set; }
}
