using Volo.Abp.Modularity;

namespace MyMechanicShop.Catalog;

public abstract class CatalogApplicationTestBase<TStartupModule> : CatalogTestBase<TStartupModule>
    where TStartupModule : IAbpModule;
