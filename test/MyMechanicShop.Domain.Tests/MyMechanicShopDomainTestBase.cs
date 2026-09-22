using Volo.Abp.Modularity;

namespace MyMechanicShop;

/* Inherit from this class for your domain layer tests. */
public abstract class MyMechanicShopDomainTestBase<TStartupModule> : MyMechanicShopTestBase<TStartupModule>
    where TStartupModule : IAbpModule;
