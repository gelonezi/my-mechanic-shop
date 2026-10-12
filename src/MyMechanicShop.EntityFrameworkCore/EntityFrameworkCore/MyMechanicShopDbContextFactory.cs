using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MyMechanicShop.EntityFrameworkCore;

public class MyMechanicShopDbContextFactory : IDesignTimeDbContextFactory<MyMechanicShopDbContext>
{
    public MyMechanicShopDbContext CreateDbContext(string[] args)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var configuration = BuildConfiguration();

        MyMechanicShopEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<MyMechanicShopDbContext>()
            .UseNpgsql(configuration.GetConnectionString("Default"));

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
