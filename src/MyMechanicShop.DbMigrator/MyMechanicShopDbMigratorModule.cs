using MyMechanicShop.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace MyMechanicShop.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(MyMechanicShopEntityFrameworkCoreModule),
    typeof(MyMechanicShopApplicationContractsModule)
)]
public class MyMechanicShopDbMigratorModule : AbpModule;
