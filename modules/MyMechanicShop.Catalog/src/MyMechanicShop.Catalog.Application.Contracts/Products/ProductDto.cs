using System;

namespace MyMechanicShop.Catalog.Products;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public float Price { get; set; }
    public int StockCount { get; set; }
}