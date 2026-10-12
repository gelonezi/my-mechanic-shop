using Volo.Abp.Application;
using Volo.Abp.Authorization;
using Volo.Abp.Modularity;

namespace MyMechanicShop.Catalog;

[DependsOn(
    typeof(CatalogDomainSharedModule),
    typeof(AbpDddApplicationContractsModule),
    typeof(AbpAuthorizationModule)
)]
public class CatalogApplicationContractsModule : AbpModule;
