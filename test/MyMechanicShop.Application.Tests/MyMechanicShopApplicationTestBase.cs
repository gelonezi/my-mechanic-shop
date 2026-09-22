using Volo.Abp.Modularity;

namespace MyMechanicShop;

public abstract class MyMechanicShopApplicationTestBase<TStartupModule> : MyMechanicShopTestBase<TStartupModule>
    where TStartupModule : IAbpModule;
