using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using MyMechanicShop.Catalog.EntityFrameworkCore;

namespace MyMechanicShop.EntityFrameworkCore;

/* Design-time factory for the Catalog module's DbContext.
 *
 * It lives in src, not in the module, because src owns every migration for every
 * database. The module's EF project stays provider-agnostic (no Sqlite, no
 * Microsoft.EntityFrameworkCore.Design, no Migrations folder).
 *
 * Generate Catalog migrations from THIS project:
 *
 *   dotnet ef migrations add <Name> --context CatalogDbContext --output-dir Migrations/Catalog
 *
 * Like MyMechanicShopDbContextFactory, this reads the DbMigrator's appsettings.json,
 * so it must be run with src/MyMechanicShop.EntityFrameworkCore as the working
 * directory or the relative base path below will not resolve.
 */
public class CatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();

        MyMechanicShopEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseSqlite(
                configuration.GetConnectionString("Catalog"),
                b => b.MigrationsAssembly(typeof(MyMechanicShopDbContext).Assembly.GetName().Name));

        return new CatalogDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../MyMechanicShop.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables();

        return builder.Build();
    }
}
