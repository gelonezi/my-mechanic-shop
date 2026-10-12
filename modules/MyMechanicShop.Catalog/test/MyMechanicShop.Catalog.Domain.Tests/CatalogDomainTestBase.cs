using Volo.Abp.Modularity;

namespace MyMechanicShop.Catalog;

public abstract class CatalogDomainTestBase<TStartupModule> : CatalogTestBase<TStartupModule>
    where TStartupModule : IAbpModule;
