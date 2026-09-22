using Microsoft.EntityFrameworkCore;
using MyMechanicShop.Catalog.Products;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace MyMechanicShop.Catalog.EntityFrameworkCore;

[ConnectionStringName(CatalogDbProperties.ConnectionStringName)]
public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : AbpDbContext<CatalogDbContext>(options), ICatalogDbContext
{
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ConfigureCatalog();
    }
}
