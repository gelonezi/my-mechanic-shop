using Volo.Abp.Modularity;

namespace MyMechanicShop.Catalog;

[DependsOn(
    typeof(CatalogApplicationModule),
    typeof(CatalogDomainTestModule)
    )]
public class CatalogApplicationTestModule : AbpModule;
