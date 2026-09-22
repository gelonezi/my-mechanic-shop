using System.Threading.Tasks;

namespace MyMechanicShop.Data;

public interface IMyMechanicShopDbSchemaMigrator
{
    Task MigrateAsync();
}
