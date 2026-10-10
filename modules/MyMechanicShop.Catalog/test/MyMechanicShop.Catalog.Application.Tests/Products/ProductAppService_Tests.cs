using System.Linq;
using System.Threading.Tasks;
using MyMechanicShop.SharedKernel.Enums;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Xunit;

namespace MyMechanicShop.Catalog.Products;

public abstract class ProductAppService_Tests<TStartupModule> : CatalogApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IProductAppService _productAppService;

    protected ProductAppService_Tests()
    {
        _productAppService = GetRequiredService<IProductAppService>();
    }

    [Fact]
    public async Task Should_Create_A_Product_And_Map_Its_Value_Objects_Back()
    {
        var created = await _productAppService.CreateAsync(new CreateUpdateProductDto
        {
            Name = "  Air filter  ",
            Brand = "Mann",
            Ean = "96385074",
            Unit = ProductUnit.Unit,
        });

        var loaded = await _productAppService.GetAsync(created.Id);

        loaded.Name.ShouldBe("Air filter");
        loaded.Brand.ShouldBe("Mann");
        loaded.Ean.ShouldBe("96385074");
        loaded.Description.ShouldBeNull();
        loaded.Unit.ShouldBe(ProductUnit.Unit);
    }

    [Fact]
    public async Task Should_Treat_Blank_Optional_Fields_As_Absent()
    {
        var created = await _productAppService.CreateAsync(new CreateUpdateProductDto
        {
            Name = "Coolant",
            Brand = "   ",
            Ean = "",
            Unit = ProductUnit.Liter,
        });

        created.Brand.ShouldBeNull();
        created.Ean.ShouldBeNull();
    }

    [Fact]
    public async Task Should_Reject_An_Invalid_Ean_As_Validation_Error()
    {
        var exception = await Should.ThrowAsync<AbpValidationException>(() =>
            _productAppService.CreateAsync(new CreateUpdateProductDto
            {
                Name = "Battery",
                Ean = "4006381333932",
                Unit = ProductUnit.Unit,
            }));

        exception.ValidationErrors.ShouldContain(e => e.MemberNames.Contains(nameof(CreateUpdateProductDto.Ean)));
    }

    [Fact]
    public async Task Should_Reject_An_Undefined_Unit_As_Validation_Error()
    {
        var exception = await Should.ThrowAsync<AbpValidationException>(() =>
            _productAppService.CreateAsync(new CreateUpdateProductDto { Name = "Battery" }));

        exception.ValidationErrors.ShouldContain(e => e.MemberNames.Contains(nameof(CreateUpdateProductDto.Unit)));
    }

    [Fact]
    public async Task Should_Sort_By_A_Value_Object_Property()
    {
        await _productAppService.CreateAsync(new CreateUpdateProductDto { Name = "Zz last", Unit = ProductUnit.Unit });
        await _productAppService.CreateAsync(new CreateUpdateProductDto { Name = "Aa first", Unit = ProductUnit.Unit });

        var page = await _productAppService.GetListAsync(new PagedAndSortedResultRequestDto
        {
            Sorting = "name desc",
            MaxResultCount = 100,
        });

        var names = page.Items.Select(p => p.Name).ToList();
        names.ShouldBe(names.OrderByDescending(n => n).ToList());
        names.First().ShouldBe("Zz last");
    }
}
