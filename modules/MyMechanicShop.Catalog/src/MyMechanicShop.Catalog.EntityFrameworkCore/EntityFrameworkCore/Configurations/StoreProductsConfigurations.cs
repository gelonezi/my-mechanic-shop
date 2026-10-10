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

            // Four decimal places: a unit price per gram or per milliliter needs more than the
            // currency's two. MonetaryVo does not round; totals are rounded when charged.
            s.ComplexProperty(x => x.Price, m =>
            {
                m.Property(v => v.Amount).HasColumnName("PriceAmount").HasPrecision(18, 4);
                m.Property(v => v.Currency).HasColumnName("PriceCurrency");
            });

            // One StoreProduct per shop and product. StoreProduct is not soft-deletable, so the
            // index needs no IsDeleted filter (which would be provider-specific SQL).
            s.HasIndex(x => new { x.TenantId, x.ProductId }).IsUnique();

            // Same database, so a real foreign key. Restrict: the catalog is soft-deleted, a
            // physical delete of a product a shop sells must fail.
            s.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).IsRequired().OnDelete(DeleteBehavior.Restrict);
        });
    }
}
