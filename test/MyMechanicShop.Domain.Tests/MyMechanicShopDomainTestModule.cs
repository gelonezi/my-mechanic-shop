using Volo.Abp.Modularity;

namespace MyMechanicShop;

[DependsOn(
    typeof(MyMechanicShopDomainModule),
    typeof(MyMechanicShopTestBaseModule)
)]
public class MyMechanicShopDomainTestModule : AbpModule;
