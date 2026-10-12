using Microsoft.EntityFrameworkCore;
using MyMechanicShop.Catalog.Products;
using MyMechanicShop.SharedKernel.ValueObjects;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace MyMechanicShop.Catalog.EntityFrameworkCore.Configurations;

internal static class ProductsConfigurations
{
    internal static void ConfigureProducts(this ModelBuilder builder)
    {
        builder.Entity<Product>(p =>
        {
            p.ToTable(CatalogDbProperties.DbTablePrefix + "Products", CatalogDbProperties.DbSchema);
            p.ConfigureByConvention();

            p.ComplexProperty(x => x.Name, n => n.Property(v => v.Value)
                .HasColumnName(nameof(Product.Name)).IsRequired().HasMaxLength(NameConsts.MaxLength));

            p.ComplexProperty(x => x.Brand, n => n.Property(v => v.Value)
                .HasColumnName(nameof(Product.Brand)).HasMaxLength(NameConsts.MaxLength));

            p.ComplexProperty(x => x.Ean, n => n.Property(v => v.Value)
                .HasColumnName(nameof(Product.Ean)).HasMaxLength(EanConsts.MaxLength));

            p.ComplexProperty(x => x.Description, n => n.Property(v => v.Value)
                .HasColumnName(nameof(Product.Description)).HasMaxLength(DescriptionConsts.MaxLength));

            p.Property(x => x.Unit).IsRequired();
        });
    }
}
