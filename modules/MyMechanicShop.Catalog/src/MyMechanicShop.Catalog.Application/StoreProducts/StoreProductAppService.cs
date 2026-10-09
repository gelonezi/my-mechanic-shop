using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using MyMechanicShop.Catalog.Permissions;
using MyMechanicShop.Catalog.Products;
using MyMechanicShop.SharedKernel.ValueObjects;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace MyMechanicShop.Catalog.StoreProducts;

/// <summary>
/// The current shop's products. Tenant-only permissions; the tenant filter scopes every
/// <see cref="StoreProduct"/> query to the current shop. The shop reads the catalog through
/// <see cref="GetAvailableProductsAsync"/>, never through the host's ProductAppService.
/// </summary>
[Authorize(CatalogPermissions.StoreProducts.Default)]
public class StoreProductAppService : CatalogAppService, IStoreProductAppService
{
    private static readonly Dictionary<string, string> SortablePaths = new(StringComparer.OrdinalIgnoreCase)
    {
        [nameof(StoreProductDto.ProductName)] = nameof(StoreProductDto.ProductName),
        [nameof(StoreProductDto.ProductBrand)] = nameof(StoreProductDto.ProductBrand),
        [nameof(StoreProductDto.ProductUnit)] = nameof(StoreProductDto.ProductUnit),
        [nameof(StoreProductDto.PriceAmount)] = nameof(StoreProductDto.PriceAmount),
        [nameof(StoreProductDto.IsActive)] = nameof(StoreProductDto.IsActive),
    };

    protected IRepository<StoreProduct, Guid> StoreProductRepository { get; }

    protected IRepository<Product, Guid> ProductRepository { get; }

    protected StoreProductManager StoreProductManager { get; }

    public StoreProductAppService(
        IRepository<StoreProduct, Guid> storeProductRepository,
        IRepository<Product, Guid> productRepository,
        StoreProductManager storeProductManager)
    {
        StoreProductRepository = storeProductRepository;
        ProductRepository = productRepository;
        StoreProductManager = storeProductManager;
    }

    public virtual async Task<PagedResultDto<StoreProductDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var query = await CreateStoreProductDtoQueryAsync();

        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query
            .OrderBy(TranslateSorting(input.Sorting))
            .PageBy(input));

        return new PagedResultDto<StoreProductDto>(totalCount, items);
    }

    [Authorize(CatalogPermissions.StoreProducts.Activate)]
    public virtual async Task<PagedResultDto<ProductDto>> GetAvailableProductsAsync(PagedAndSortedResultRequestDto input)
    {
        var activeProductIds = (await StoreProductRepository.GetQueryableAsync())
            .Where(s => s.IsActive)
            .Select(s => s.ProductId);

        var query = (await ProductRepository.GetQueryableAsync())
            .Where(p => !activeProductIds.Contains(p.Id));

        var totalCount = await AsyncExecuter.CountAsync(query);
        var products = await AsyncExecuter.ToListAsync(query
            .OrderBy(p => p.Name.Value)
            .PageBy(input));

        return new PagedResultDto<ProductDto>(totalCount, ObjectMapper.Map<List<Product>, List<ProductDto>>(products));
    }

    [Authorize(CatalogPermissions.StoreProducts.Activate)]
    public virtual async Task<StoreProductDto> ActivateAsync(ActivateStoreProductDto input)
    {
        var storeProduct = await StoreProductManager.ActivateAsync(
            input.ProductId,
            MonetaryVo.Create(input.PriceAmount, input.PriceCurrency));

        return await GetDtoAsync(storeProduct.Id);
    }

    [Authorize(CatalogPermissions.StoreProducts.Edit)]
    public virtual async Task<StoreProductDto> UpdatePriceAsync(Guid id, StorePriceDto input)
    {
        var storeProduct = await StoreProductRepository.GetAsync(id);
        storeProduct.SetPrice(MonetaryVo.Create(input.PriceAmount, input.PriceCurrency));
        await StoreProductRepository.UpdateAsync(storeProduct, autoSave: true);

        return await GetDtoAsync(id);
    }

    [Authorize(CatalogPermissions.StoreProducts.Deactivate)]
    public virtual async Task<StoreProductDto> DeactivateAsync(Guid id)
    {
        var storeProduct = await StoreProductRepository.GetAsync(id);
        storeProduct.Deactivate();
        await StoreProductRepository.UpdateAsync(storeProduct, autoSave: true);

        return await GetDtoAsync(id);
    }

    /// <summary>
    /// Store products joined with their catalog product. Both tables are in the Catalog database,
    /// so this is one SQL query. Products the host soft-deleted drop out through the join.
    /// </summary>
    protected virtual async Task<IQueryable<StoreProductDto>> CreateStoreProductDtoQueryAsync()
    {
        var storeProducts = await StoreProductRepository.GetQueryableAsync();
        var products = await ProductRepository.GetQueryableAsync();

        return storeProducts.Join(
            products,
            s => s.ProductId,
            p => p.Id,
            (s, p) => new StoreProductDto
            {
                Id = s.Id,
                ProductId = p.Id,
                ProductName = p.Name.Value,
                ProductBrand = p.Brand == null ? null : p.Brand.Value,
                ProductUnit = p.Unit,
                PriceAmount = s.Price.Amount,
                PriceCurrency = s.Price.Currency,
                IsActive = s.IsActive,
            });
    }

    protected virtual async Task<StoreProductDto> GetDtoAsync(Guid id)
    {
        var query = await CreateStoreProductDtoQueryAsync();
        return await AsyncExecuter.SingleAsync(query.Where(x => x.Id == id));
    }

    private static string TranslateSorting(string? sorting)
    {
        if (string.IsNullOrWhiteSpace(sorting))
        {
            return nameof(StoreProductDto.ProductName);
        }

        return string.Join(", ", sorting
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part =>
            {
                var tokens = part.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                // Unknown fields fall back to the product name rather than reaching dynamic LINQ.
                var path = SortablePaths.GetValueOrDefault(tokens[0], nameof(StoreProductDto.ProductName));
                return tokens.Length > 1 ? $"{path} {tokens[1]}" : path;
            }));
    }
}
