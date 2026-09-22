using Volo.Abp.Modularity;

namespace MyMechanicShop.Catalog;

[DependsOn(
    typeof(CatalogDomainModule),
    typeof(CatalogTestBaseModule)
)]
public class CatalogDomainTestModule : AbpModule;
