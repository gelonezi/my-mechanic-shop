using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MyMechanicShop.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class MyMechanicShopDbContextFactory : IDesignTimeDbContextFactory<MyMechanicShopDbContext>
{
    public MyMechanicShopDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        MyMechanicShopEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<MyMechanicShopDbContext>()
            .UseSqlite(configuration.GetConnectionString("Default"));
        
        return new MyMechanicShopDbContext(builder.Options);
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
