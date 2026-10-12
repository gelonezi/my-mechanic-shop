using System.Threading.Tasks;

namespace MyMechanicShop.Catalog.Data;

public interface ICatalogDbSchemaMigrator
{
    Task MigrateAsync();
}
