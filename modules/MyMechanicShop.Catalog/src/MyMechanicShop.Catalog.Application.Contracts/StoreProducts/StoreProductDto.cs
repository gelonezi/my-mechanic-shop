using System;
using MyMechanicShop.SharedKernel.Enums;
using Volo.Abp.Application.Dtos;

namespace MyMechanicShop.Catalog.StoreProducts;

/// <summary>A product of the current shop: the catalog data it needs to show, plus the shop's price.</summary>
public class StoreProductDto : EntityDto<Guid>
{
    public Guid ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string? ProductBrand { get; set; }

    public ProductUnit ProductUnit { get; set; }

    public decimal PriceAmount { get; set; }

    public Currency PriceCurrency { get; set; }

    public bool IsActive { get; set; }
}
