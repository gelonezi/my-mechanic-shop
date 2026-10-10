using System;
using System.Threading.Tasks;
using MyMechanicShop.SharedKernel.Enums;
using MyMechanicShop.SharedKernel.ValueObjects;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
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
    private readonly IGuidGenerator _guidGenerator;

    protected ProductRepository_Tests()
    {
        _productRepository = GetRequiredService<IRepository<Product, Guid>>();
        _guidGenerator = GetRequiredService<IGuidGenerator>();
    }

    [Fact]
    public async Task Should_Persist_Every_Value_Object()
    {
        var inserted = await WithUnitOfWorkAsync(() => _productRepository.InsertAsync(
            new Product(_guidGenerator.Create(), NameVo.Create("Spark plug"), ProductUnit.Unit)
                .SetBrand(NameVo.Create("NGK"))
                .SetEan(EanVo.Create("4006381333931"))
                .SetDescription(DescriptionVo.Create("Iridium spark plug."))));

        var loaded = await WithUnitOfWorkAsync(() => _productRepository.GetAsync(inserted.Id));

        loaded.Name.Value.ShouldBe("Spark plug");
        loaded.Brand.ShouldNotBeNull().Value.ShouldBe("NGK");
        loaded.Ean.ShouldNotBeNull().Value.ShouldBe("4006381333931");
        loaded.Ean.Format.ShouldBe(EanFormat.Ean13);
        loaded.Description.ShouldNotBeNull().Value.ShouldBe("Iridium spark plug.");
        loaded.Unit.ShouldBe(ProductUnit.Unit);
    }

    [Fact]
    public async Task Should_Read_Back_Missing_Optional_Value_Objects_As_Null()
    {
        var inserted = await WithUnitOfWorkAsync(() => _productRepository.InsertAsync(
            new Product(_guidGenerator.Create(), NameVo.Create("Brake fluid"), ProductUnit.Milliliter)));

        var loaded = await WithUnitOfWorkAsync(() => _productRepository.GetAsync(inserted.Id));

        loaded.Brand.ShouldBeNull();
        loaded.Ean.ShouldBeNull();
        loaded.Description.ShouldBeNull();
    }

    [Fact]
    public void Should_Reject_An_Undefined_Unit()
    {
        Should.Throw<ArgumentException>(() =>
            new Product(_guidGenerator.Create(), NameVo.Create("Wiper blade"), ProductUnit.Undefined));
    }
}
