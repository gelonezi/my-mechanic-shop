using System;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Xunit;

namespace MyMechanicShop.Catalog.Products;

/* Write repository tests in this project, as abstract classes.
 * Then inherit these abstract classes from EF Core & MongoDB test projects.
 * In this way, both database providers are tested with the same set of tests.
 */
public abstract class ProductRepository_Tests<TStartupModule> : CatalogDomainTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IRepository<Product, Guid> _productRepository;

    protected ProductRepository_Tests()
    {
        _productRepository = GetRequiredService<IRepository<Product, Guid>>();
    }

    [Fact]
    public async Task Should_Insert_And_Read_Back_A_Product()
    {
        var inserted = await WithUnitOfWorkAsync(() => _productRepository.InsertAsync(
            new Product { Name = "Oil filter", Price = 12.5f, StockCount = 3 }));

        var loaded = await WithUnitOfWorkAsync(() => _productRepository.GetAsync(inserted.Id));

        loaded.Name.ShouldBe("Oil filter");
        loaded.Price.ShouldBe(12.5f);
        loaded.StockCount.ShouldBe(3);
    }
}
