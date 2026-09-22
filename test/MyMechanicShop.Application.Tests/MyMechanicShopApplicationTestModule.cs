using Volo.Abp.Modularity;

namespace MyMechanicShop;

[DependsOn(
    typeof(MyMechanicShopApplicationModule),
    typeof(MyMechanicShopDomainTestModule)
)]
public class MyMechanicShopApplicationTestModule : AbpModule;
