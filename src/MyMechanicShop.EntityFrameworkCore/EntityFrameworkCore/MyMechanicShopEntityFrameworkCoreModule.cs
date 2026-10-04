using MyMechanicShop.Catalog.EntityFrameworkCore;
using System;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using Volo.Abp.Studio;

namespace MyMechanicShop.EntityFrameworkCore;

[DependsOn(
    typeof(CatalogEntityFrameworkCoreModule),
    typeof(MyMechanicShopDomainModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule),
    typeof(AbpSettingManagementEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCorePostgreSqlModule),
    typeof(AbpBackgroundJobsEntityFrameworkCoreModule),
    typeof(AbpAuditLoggingEntityFrameworkCoreModule),
    typeof(AbpFeatureManagementEntityFrameworkCoreModule),
    typeof(AbpIdentityEntityFrameworkCoreModule),
    typeof(AbpOpenIddictEntityFrameworkCoreModule),
    typeof(AbpTenantManagementEntityFrameworkCoreModule),
    typeof(BlobStoringDatabaseEntityFrameworkCoreModule)
    )]
public class MyMechanicShopEntityFrameworkCoreModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        // https://www.npgsql.org/efcore/release-notes/6.0.html#opting-out-of-the-new-timestamp-mapping-logic
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        MyMechanicShopEfCoreEntityExtensionMappings.Configure();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<MyMechanicShopDbContext>(options =>
        {
                /* Remove "includeAllEntities: true" to create
                 * default repositories only for aggregate roots */
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        if (AbpStudioAnalyzeHelper.IsInAnalyzeMode)
        {
            return;
        }

        Configure<AbpDbContextOptions>(options =>
        {
            /* The main point to change your DBMS.
             * See also MyMechanicShopDbContextFactory for EF Core tooling. */

            /* Every DbContext — this app's and every module's — looks for its migrations
             * in THIS assembly. Module EF projects stay provider-agnostic and own no
             * Migrations folder, so src is the single place that describes the schema of
             * every database and the DbMigrator can bring a whole environment up to date
             * from its connection strings alone.
             *
             * EF otherwise looks for migrations next to the DbContext class, which for a
             * module context is the module assembly, where it finds none — it then creates
             * an empty database and applies nothing, silently.
             *
             * Adding a module: give it a connection string, add an
             * IMyMechanicShopDbSchemaMigrator for its DbContext, and generate its
             * migrations into Migrations/<Module>/ here. Nothing to change in this block. */
            options.UseNpgsql(b =>
                b.MigrationsAssembly(typeof(MyMechanicShopDbContext).Assembly.GetName().Name));
        });
    }
}
