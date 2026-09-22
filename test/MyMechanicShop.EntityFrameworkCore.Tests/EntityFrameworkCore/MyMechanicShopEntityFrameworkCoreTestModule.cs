using Microsoft.EntityFrameworkCore;
using MyMechanicShop.Catalog;
using MyMechanicShop.Catalog.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Uow;

namespace MyMechanicShop.EntityFrameworkCore;

[DependsOn(
    typeof(MyMechanicShopApplicationTestModule),
    typeof(MyMechanicShopEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqliteModule)
)]
public class MyMechanicShopEntityFrameworkCoreTestModule : AbpModule
{
    private AbpUnitTestSqliteDatabase? _database;
    private AbpUnitTestSqliteDatabase? _catalogDatabase;

    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<AbpSqliteOptions>(x => x.BusyTimeout = null);
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<FeatureManagementOptions>(options =>
        {
            options.SaveStaticFeaturesToDatabase = false;
            options.IsDynamicFeatureStoreEnabled = false;
        });
        Configure<PermissionManagementOptions>(options =>
        {
            options.SaveStaticPermissionsToDatabase = false;
            options.IsDynamicPermissionStoreEnabled = false;
        });
        context.Services.AddAlwaysDisableUnitOfWorkTransaction();

        ConfigureInMemorySqlite(context.Services);

    }

    private void ConfigureInMemorySqlite(IServiceCollection services)
    {
        _database = new AbpUnitTestSqliteDatabase();
        _database.CreateTables(
            new MyMechanicShopDbContext(new DbContextOptionsBuilder<MyMechanicShopDbContext>().UseSqlite(_database.ConnectionString).Options));

        /* One in-memory database PER MODULE, mirroring the real topology where each module
         * owns its own database. Sharing a single database here would let a test join
         * across a module boundary and pass, hiding coupling that would fail in production
         * — especially once modules talk over events instead of tables.
         *
         * No migrations are involved: tables are created directly from each model.
         *
         * Adding a module: new AbpUnitTestSqliteDatabase, CreateTables for its DbContext,
         * map its ConnectionStringName below, and dispose it in OnApplicationShutdown. */
        _catalogDatabase = new AbpUnitTestSqliteDatabase();
        _catalogDatabase.CreateTables(
            new CatalogDbContext(new DbContextOptionsBuilder<CatalogDbContext>().UseSqlite(_catalogDatabase.ConnectionString).Options));

        services.Configure<AbpDbConnectionOptions>(options =>
        {
            options.ConnectionStrings.Default = _database.ConnectionString;
            options.ConnectionStrings[CatalogDbProperties.ConnectionStringName] = _catalogDatabase.ConnectionString;
        });

        services.Configure<AbpDbContextOptions>(options =>
        {
            options.Configure(context =>
            {
                context.UseSqlite();
            });
        });
    }

    public override void OnApplicationShutdown(ApplicationShutdownContext context)
    {
        _database?.Dispose();
        _catalogDatabase?.Dispose();
    }
}
