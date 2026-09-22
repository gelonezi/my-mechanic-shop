using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace MyMechanicShop.Catalog;

[DependsOn(
    typeof(AbpVirtualFileSystemModule)
    )]
public class CatalogInstallerModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<CatalogInstallerModule>();
        });
    }
}
