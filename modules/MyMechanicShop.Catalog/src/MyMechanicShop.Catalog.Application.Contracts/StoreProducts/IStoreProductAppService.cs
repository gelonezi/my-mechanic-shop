using System;
using System.Threading.Tasks;
using MyMechanicShop.Catalog.Products;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace MyMechanicShop.Catalog.StoreProducts;

/// <summary>The current shop's products. Tenant side only (see CatalogPermissions.StoreProducts).</summary>
public interface IStoreProductAppService : IApplicationService
{
    /// <summary>The shop's products, active and deactivated.</summary>
    Task<PagedResultDto<StoreProductDto>> GetListAsync(PagedAndSortedResultRequestDto input);

    /// <summary>Catalog products the shop is not selling yet — the ones it can activate.</summary>
    Task<PagedResultDto<ProductDto>> GetAvailableProductsAsync(PagedAndSortedResultRequestDto input);

    /// <summary>Starts selling a catalog product, or reactivates one the shop had deactivated.</summary>
    Task<StoreProductDto> ActivateAsync(ActivateStoreProductDto input);

    Task<StoreProductDto> UpdatePriceAsync(Guid id, StorePriceDto input);

    Task<StoreProductDto> DeactivateAsync(Guid id);
}
