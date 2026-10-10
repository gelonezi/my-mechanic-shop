using System;
using MyMechanicShop.SharedKernel.Enums;
using Volo.Abp.Application.Dtos;

namespace MyMechanicShop.Catalog.Products;

public class ProductDto : EntityDto<Guid>
{
    public string Name { get; set; } = null!;

    public string? Brand { get; set; }

    public string? Ean { get; set; }

    public string? Description { get; set; }

    public ProductUnit Unit { get; set; }
}
