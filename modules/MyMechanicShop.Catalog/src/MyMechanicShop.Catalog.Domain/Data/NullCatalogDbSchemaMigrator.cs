using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace MyMechanicShop.Catalog.Data;

public class NullCatalogDbSchemaMigrator : ICatalogDbSchemaMigrator, ITransientDependency
{
    public virtual Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
