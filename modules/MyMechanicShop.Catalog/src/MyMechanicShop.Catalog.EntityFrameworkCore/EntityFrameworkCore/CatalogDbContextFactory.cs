using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MyMechanicShop.Catalog.EntityFrameworkCore;

public class CatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        var builder = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseNpgsql(configuration.GetConnectionString(CatalogDbProperties.ConnectionStringName));

        return new CatalogDbContext(builder.Options);
    }
}
