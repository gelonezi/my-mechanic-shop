using Microsoft.EntityFrameworkCore;
using MyMechanicShop.Catalog.EntityFrameworkCore.Configurations;
using Volo.Abp;

namespace MyMechanicShop.Catalog.EntityFrameworkCore;

public static class CatalogDbContextModelCreatingExtensions
{
    public static void ConfigureCatalog(
        this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));
        builder.ConfigureProducts();
        builder.ConfigureStoreProducts();
    }
}
