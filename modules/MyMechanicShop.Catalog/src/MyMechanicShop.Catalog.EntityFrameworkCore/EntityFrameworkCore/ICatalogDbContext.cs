using Microsoft.EntityFrameworkCore;
using MyMechanicShop.Catalog.Products;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace MyMechanicShop.Catalog.EntityFrameworkCore;

[ConnectionStringName(CatalogDbProperties.ConnectionStringName)]
public interface ICatalogDbContext : IEfCoreDbContext;
