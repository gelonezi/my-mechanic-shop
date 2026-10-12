using System;
using System.Threading.Tasks;
using MyMechanicShop.Catalog.Products;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace MyMechanicShop.Catalog.StoreProducts;

public interface IStoreProductAppService : IApplicationService
{
    Task<PagedResultDto<StoreProductDto>> GetListAsync(PagedAndSortedResultRequestDto input);

    Task<PagedResultDto<ProductDto>> GetAvailableProductsAsync(PagedAndSortedResultRequestDto input);

    Task<StoreProductDto> ActivateAsync(ActivateStoreProductDto input);

    Task<StoreProductDto> UpdatePriceAsync(Guid id, StorePriceDto input);

    Task<StoreProductDto> DeactivateAsync(Guid id);
}
