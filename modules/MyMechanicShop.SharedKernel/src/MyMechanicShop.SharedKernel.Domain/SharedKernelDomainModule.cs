using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace MyMechanicShop.SharedKernel;

[DependsOn(
    typeof(SharedKernelDomainSharedModule),
    typeof(AbpDddDomainModule)
)]
public class SharedKernelDomainModule : AbpModule
{

}
