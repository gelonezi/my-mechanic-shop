using System;
using System.ComponentModel.DataAnnotations;

namespace MyMechanicShop.Catalog.Products;

public class CreateUpdateProductDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public float Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockCount { get; set; }
}