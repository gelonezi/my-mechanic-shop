using Microsoft.EntityFrameworkCore;
using MyMechanicShop.Catalog.Products;
using MyMechanicShop.Catalog.StoreProducts;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace MyMechanicShop.Catalog.EntityFrameworkCore.Configurations;

internal static class StoreProductsConfigurations
{
    internal static void ConfigureStoreProducts(this ModelBuilder builder)
    {
        builder.Entity<StoreProduct>(s =>
        {
            s.ToTable(CatalogDbProperties.DbTablePrefix + "StoreProducts", CatalogDbProperties.DbSchema);
            s.ConfigureByConvention();

            s.ComplexProperty(x => x.Price, m =>
            {
                m.Property(v => v.Amount).HasColumnName("PriceAmount").HasPrecision(18, 4);
                m.Property(v => v.Currency).HasColumnName("PriceCurrency");
            });

            s.HasIndex(x => new { x.TenantId, x.ProductId }).IsUnique();

            s.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).IsRequired().OnDelete(DeleteBehavior.Restrict);
        });
    }
}
