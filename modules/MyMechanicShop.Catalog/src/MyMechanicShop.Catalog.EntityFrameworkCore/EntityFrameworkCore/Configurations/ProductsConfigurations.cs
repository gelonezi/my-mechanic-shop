using Microsoft.EntityFrameworkCore;
using MyMechanicShop.Catalog.Products;
using Volo.Abp.EntityFrameworkCore.Modeling;
namespace MyMechanicShop.Catalog.EntityFrameworkCore.Configurations;

internal static class ProductsConfigurations
{
    internal static void ConfigureProducts(this ModelBuilder builder)
    {
        builder.Entity<Product>(p =>
        {
            p.ToTable(CatalogDbProperties.DbTablePrefix + "Products",
                CatalogDbProperties.DbSchema);

            p.ConfigureByConvention();
            p.Property(n => n.Price).IsRequired();
            p.Property(n => n.Name).IsRequired().HasMaxLength(100);
        });
    }
}