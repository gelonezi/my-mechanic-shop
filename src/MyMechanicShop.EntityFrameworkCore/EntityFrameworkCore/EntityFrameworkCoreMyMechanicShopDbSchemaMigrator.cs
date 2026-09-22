using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyMechanicShop.Data;
using Volo.Abp.DependencyInjection;

namespace MyMechanicShop.EntityFrameworkCore;

public class EntityFrameworkCoreMyMechanicShopDbSchemaMigrator
    : IMyMechanicShopDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreMyMechanicShopDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the MyMechanicShopDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<MyMechanicShopDbContext>()
            .Database
            .MigrateAsync();
    }
}
