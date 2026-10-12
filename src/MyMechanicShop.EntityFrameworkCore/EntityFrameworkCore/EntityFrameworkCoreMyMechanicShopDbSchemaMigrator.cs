using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyMechanicShop.Data;
using Volo.Abp.DependencyInjection;

namespace MyMechanicShop.EntityFrameworkCore;

public class EntityFrameworkCoreMyMechanicShopDbSchemaMigrator(IServiceProvider serviceProvider) : IMyMechanicShopDbSchemaMigrator, ITransientDependency
{
    public async Task MigrateAsync()
    {
        await serviceProvider
            .GetRequiredService<MyMechanicShopDbContext>()
            .Database
            .MigrateAsync();
    }
}
