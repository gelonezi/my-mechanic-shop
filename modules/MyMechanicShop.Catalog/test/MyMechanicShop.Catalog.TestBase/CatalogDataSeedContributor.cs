using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;

namespace MyMechanicShop.Catalog;

public class CatalogDataSeedContributor(ICurrentTenant currentTenant) : IDataSeedContributor, ITransientDependency
{
    public Task SeedAsync(DataSeedContext context)
    {
        using (currentTenant.Change(context.TenantId))
        {
            return Task.CompletedTask;
        }
    }
}
