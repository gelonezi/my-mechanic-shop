using MyMechanicShop.SharedKernel;
using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace MyMechanicShop.Catalog;

[DependsOn(
    typeof(SharedKernelDomainModule),
    typeof(AbpDddDomainModule),
    typeof(CatalogDomainSharedModule)
)]
public class CatalogDomainModule : AbpModule;
