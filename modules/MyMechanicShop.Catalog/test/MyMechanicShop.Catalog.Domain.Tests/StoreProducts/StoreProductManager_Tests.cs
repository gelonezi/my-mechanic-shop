using System;
using System.Threading.Tasks;
using MyMechanicShop.Catalog.Products;
using MyMechanicShop.SharedKernel.Enums;
using MyMechanicShop.SharedKernel.ValueObjects;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace MyMechanicShop.Catalog.StoreProducts;

public abstract class StoreProductManager_Tests<TStartupModule> : CatalogDomainTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private static readonly Guid ShopA = Guid.NewGuid();
    private static readonly Guid ShopB = Guid.NewGuid();

    private readonly StoreProductManager _manager;
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<StoreProduct, Guid> _storeProductRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IGuidGenerator _guidGenerator;

    protected StoreProductManager_Tests()
    {
        _manager = GetRequiredService<StoreProductManager>();
        _productRepository = GetRequiredService<IRepository<Product, Guid>>();
        _storeProductRepository = GetRequiredService<IRepository<StoreProduct, Guid>>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
        _guidGenerator = GetRequiredService<IGuidGenerator>();
    }

    [Fact]
    public async Task Should_Activate_A_Product_In_The_Current_Shop()
    {
        var productId = await CreateCatalogProductAsync();

        var storeProduct = await InShopAsync(ShopA, () =>
            _manager.ActivateAsync(productId, MonetaryVo.Create(49.90m, Currency.Brl)));

        storeProduct.TenantId.ShouldBe(ShopA);
        storeProduct.ProductId.ShouldBe(productId);
        storeProduct.Price.Amount.ShouldBe(49.90m);
        storeProduct.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Not_Activate_A_Product_Twice_In_The_Same_Shop()
    {
        var productId = await CreateCatalogProductAsync();
        await InShopAsync(ShopA, () => _manager.ActivateAsync(productId, MonetaryVo.Create(10m, Currency.Brl)));

        var exception = await Should.ThrowAsync<BusinessException>(() => InShopAsync(ShopA, () =>
            _manager.ActivateAsync(productId, MonetaryVo.Create(12m, Currency.Brl))));

        exception.Code.ShouldBe(CatalogErrorCodes.ProductAlreadyActive);
    }

    [Fact]
    public async Task Should_Let_Each_Shop_Activate_The_Same_Product_At_Its_Own_Price()
    {
        var productId = await CreateCatalogProductAsync();

        var inShopA = await InShopAsync(ShopA, () => _manager.ActivateAsync(productId, MonetaryVo.Create(10m, Currency.Brl)));
        var inShopB = await InShopAsync(ShopB, () => _manager.ActivateAsync(productId, MonetaryVo.Create(15m, Currency.Brl)));

        inShopA.Id.ShouldNotBe(inShopB.Id);
        inShopB.Price.Amount.ShouldBe(15m);
    }

    [Fact]
    public async Task Should_Reactivate_A_Deactivated_Product_With_The_New_Price()
    {
        var productId = await CreateCatalogProductAsync();
        var first = await InShopAsync(ShopA, () => _manager.ActivateAsync(productId, MonetaryVo.Create(10m, Currency.Brl)));
        await InShopAsync(ShopA, async () =>
        {
            var storeProduct = await _storeProductRepository.GetAsync(first.Id);
            await _storeProductRepository.UpdateAsync(storeProduct.Deactivate(), autoSave: true);
        });

        var reactivated = await InShopAsync(ShopA, () =>
            _manager.ActivateAsync(productId, MonetaryVo.Create(11m, Currency.Brl)));

        reactivated.Id.ShouldBe(first.Id);
        reactivated.IsActive.ShouldBeTrue();
        reactivated.Price.Amount.ShouldBe(11m);
    }

    [Fact]
    public async Task Should_Not_Activate_A_Product_Missing_From_The_Catalog()
    {
        await Should.ThrowAsync<EntityNotFoundException>(() => InShopAsync(ShopA, () =>
            _manager.ActivateAsync(Guid.NewGuid(), MonetaryVo.Create(10m, Currency.Brl))));
    }

    private Task<Guid> CreateCatalogProductAsync()
    {
        return WithUnitOfWorkAsync(async () => (await _productRepository.InsertAsync(
            new Product(_guidGenerator.Create(), NameVo.Create("Oil filter"), ProductUnit.Unit))).Id);
    }

    private Task<T> InShopAsync<T>(Guid tenantId, Func<Task<T>> action)
    {
        return WithUnitOfWorkAsync(async () =>
        {
            using (_currentTenant.Change(tenantId))
            {
                return await action();
            }
        });
    }

    private Task InShopAsync(Guid tenantId, Func<Task> action)
    {
        return WithUnitOfWorkAsync(async () =>
        {
            using (_currentTenant.Change(tenantId))
            {
                await action();
            }
        });
    }
}
