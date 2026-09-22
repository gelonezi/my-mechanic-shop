using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyMechanicShop.Catalog.EntityFrameworkCore;
using MyMechanicShop.Data;
using Volo.Abp.DependencyInjection;

namespace MyMechanicShop.EntityFrameworkCore;

/* Migrates the Catalog module's own database.
 *
 * MyMechanicShopDbMigrationService resolves every IMyMechanicShopDbSchemaMigrator and
 * calls them in turn, so registering one per module DbContext is what lets a single
 * DbMigrator run bring a whole environment up to date. Add one of these for each new
 * module database.
 *
 * Which database this hits comes from [ConnectionStringName("Catalog")] on
 * CatalogDbContext, resolved against the running app's connection strings.
 */
/* [ExposeServices] is REQUIRED, not decoration. ABP's conventional registrar only
 * exposes a class via its "default" interfaces — IFoo counts only when the class name
 * ENDS WITH Foo. EntityFrameworkCoreMyMechanicShopDbSchemaMigrator ends with
 * MyMechanicShopDbSchemaMigrator so it is exposed automatically; this class ends with
 * CatalogDbSchemaMigrator, so without the attribute it registers only as itself, the
 * migration service's IEnumerable<IMyMechanicShopDbSchemaMigrator> never contains it,
 * and the Catalog database is silently left empty. Same applies to every future module
 * migrator. */
[ExposeServices(typeof(IMyMechanicShopDbSchemaMigrator))]
public class EntityFrameworkCoreCatalogDbSchemaMigrator
    : IMyMechanicShopDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreCatalogDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* Resolved from IServiceProvider rather than injected directly so the
         * current tenant's connection string is picked up in the current scope —
         * same reasoning as EntityFrameworkCoreMyMechanicShopDbSchemaMigrator. */

        await _serviceProvider
            .GetRequiredService<CatalogDbContext>()
            .Database
            .MigrateAsync();
    }
}
