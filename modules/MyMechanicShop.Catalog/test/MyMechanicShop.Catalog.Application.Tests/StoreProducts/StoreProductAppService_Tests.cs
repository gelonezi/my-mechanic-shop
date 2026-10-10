using System;
using System.Threading.Tasks;
using MyMechanicShop.Catalog.Products;
using MyMechanicShop.SharedKernel.Enums;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Validation;
using Xunit;

namespace MyMechanicShop.Catalog.StoreProducts;

public abstract class StoreProductAppService_Tests<TStartupModule> : CatalogApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private static readonly Guid Shop = Guid.NewGuid();

    private readonly IStoreProductAppService _storeProductAppService;
    private readonly IProductAppService _productAppService;
    private readonly ICurrentTenant _currentTenant;

    protected StoreProductAppService_Tests()
    {
        _storeProductAppService = GetRequiredService<IStoreProductAppService>();
        _productAppService = GetRequiredService<IProductAppService>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
    }

    [Fact]
    public async Task Should_Activate_And_List_With_The_Catalog_Data()
    {
        var product = await CreateCatalogProductAsync("Timing belt kit", "Gates", ProductUnit.Kit);

        using (_currentTenant.Change(Shop))
        {
            var activated = await _storeProductAppService.ActivateAsync(new ActivateStoreProductDto
            {
                ProductId = product.Id,
                PriceAmount = 389.90m,
            });

            activated.ProductName.ShouldBe("Timing belt kit");
            activated.ProductBrand.ShouldBe("Gates");
            activated.ProductUnit.ShouldBe(ProductUnit.Kit);
            activated.PriceAmount.ShouldBe(389.90m);
            activated.PriceCurrency.ShouldBe(Currency.Brl);

            var list = await _storeProductAppService.GetListAsync(new PagedAndSortedResultRequestDto { MaxResultCount = 100 });
            list.Items.ShouldContain(s => s.Id == activated.Id);
        }
    }

    [Fact]
    public async Task Available_Products_Should_Exclude_The_Ones_Already_Active()
    {
        var product = await CreateCatalogProductAsync("Wheel bearing", null, ProductUnit.Unit);

        using (_currentTenant.Change(Shop))
        {
            (await GetAvailableAsync()).Items.ShouldContain(p => p.Id == product.Id);

            var activated = await _storeProductAppService.ActivateAsync(new ActivateStoreProductDto
            {
                ProductId = product.Id,
                PriceAmount = 120m,
            });
            (await GetAvailableAsync()).Items.ShouldNotContain(p => p.Id == product.Id);

            await _storeProductAppService.DeactivateAsync(activated.Id);
            (await GetAvailableAsync()).Items.ShouldContain(p => p.Id == product.Id);
        }
    }

    [Fact]
    public async Task Should_Change_The_Price()
    {
        var product = await CreateCatalogProductAsync("Clutch kit", "LuK", ProductUnit.Kit);

        using (_currentTenant.Change(Shop))
        {
            var activated = await _storeProductAppService.ActivateAsync(new ActivateStoreProductDto
            {
                ProductId = product.Id,
                PriceAmount = 900m,
            });

            var updated = await _storeProductAppService.UpdatePriceAsync(activated.Id, new StorePriceDto
            {
                PriceAmount = 950.50m,
                PriceCurrency = Currency.Brl,
            });

            updated.PriceAmount.ShouldBe(950.50m);
        }
    }

    [Fact]
    public async Task Should_Reject_A_Negative_Price_As_Validation_Error()
    {
        var product = await CreateCatalogProductAsync("Headlamp bulb", null, ProductUnit.Pair);

        using (_currentTenant.Change(Shop))
        {
            await Should.ThrowAsync<AbpValidationException>(() =>
                _storeProductAppService.ActivateAsync(new ActivateStoreProductDto
                {
                    ProductId = product.Id,
                    PriceAmount = -1m,
                }));
        }
    }

    private Task<ProductDto> CreateCatalogProductAsync(string name, string? brand, ProductUnit unit)
    {
        // The catalog is host data: created outside any tenant.
        return _productAppService.CreateAsync(new CreateUpdateProductDto { Name = name, Brand = brand, Unit = unit });
    }

    private Task<PagedResultDto<ProductDto>> GetAvailableAsync()
    {
        return _storeProductAppService.GetAvailableProductsAsync(new PagedAndSortedResultRequestDto { MaxResultCount = 1000 });
    }
}
