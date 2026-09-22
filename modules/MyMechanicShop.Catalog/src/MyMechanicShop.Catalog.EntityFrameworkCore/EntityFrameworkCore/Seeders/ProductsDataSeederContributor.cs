using System;
using System.Threading.Tasks;
using MyMechanicShop.Catalog.Products;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace MyMechanicShop.Catalog.EntityFrameworkCore.Seeders;

public class ProductsDataSeederContributor(IRepository<Product, Guid> productsRepository) : IDataSeedContributor, ITransientDependency
{
    public async Task SeedAsync(DataSeedContext context)
    {
        if (await productsRepository.GetCountAsync() > 0)
            return;

        await productsRepository.InsertAsync(
            new Product
            {
                Name = "Motor Oil 900ml",
                Price = 8,
                StockCount = 12
            },
            autoSave: true
        );

        await productsRepository.InsertAsync(
            new Product
            {
                Name = "Tire 14\"",
                Price = 50,
                StockCount = 4
            },
            autoSave: true
        );
    }
}