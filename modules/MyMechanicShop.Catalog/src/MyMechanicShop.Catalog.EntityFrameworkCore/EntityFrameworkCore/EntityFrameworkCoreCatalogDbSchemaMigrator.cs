using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyMechanicShop.Catalog.Data;
using Volo.Abp.DependencyInjection;

namespace MyMechanicShop.Catalog.EntityFrameworkCore;

public class EntityFrameworkCoreCatalogDbSchemaMigrator(IServiceProvider serviceProvider) : ICatalogDbSchemaMigrator, ITransientDependency
{
    public virtual async Task MigrateAsync()
    {
        await serviceProvider
            .GetRequiredService<CatalogDbContext>()
            .Database
            .MigrateAsync();
    }
}
