using Volo.Abp.Modularity;

namespace MyMechanicShop;

public abstract class MyMechanicShopDomainTestBase<TStartupModule> : MyMechanicShopTestBase<TStartupModule>
    where TStartupModule : IAbpModule;
