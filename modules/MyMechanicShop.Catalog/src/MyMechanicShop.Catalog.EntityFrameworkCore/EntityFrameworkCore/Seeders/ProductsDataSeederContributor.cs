using System;
using System.Threading.Tasks;
using MyMechanicShop.Catalog.Products;
using MyMechanicShop.SharedKernel.Enums;
using MyMechanicShop.SharedKernel.ValueObjects;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace MyMechanicShop.Catalog.EntityFrameworkCore.Seeders;

/// <summary>
/// Sample products for the global catalog. The data seeder runs once for the host and once per
/// tenant; the catalog is host data, so tenant runs are skipped.
/// </summary>
public class ProductsDataSeederContributor(
    IRepository<Product, Guid> productsRepository,
    IGuidGenerator guidGenerator) : IDataSeedContributor, ITransientDependency
{
    public async Task SeedAsync(DataSeedContext context)
    {
        if (context.TenantId is not null || await productsRepository.GetCountAsync() > 0)
        {
            return;
        }

        await productsRepository.InsertAsync(
            new Product(guidGenerator.Create(), NameVo.Create("Motor Oil 5W-30 1L"), ProductUnit.Unit)
                .SetBrand(NameVo.Create("Castrol")),
            autoSave: true);

        await productsRepository.InsertAsync(
            new Product(guidGenerator.Create(), NameVo.Create("Tire 175/70 R14"), ProductUnit.Unit)
                .SetDescription(DescriptionVo.Create("Passenger car tire, rim 14\".")),
            autoSave: true);
    }
}
