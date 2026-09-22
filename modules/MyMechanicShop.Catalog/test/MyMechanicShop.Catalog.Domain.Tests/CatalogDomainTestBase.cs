using Volo.Abp.Modularity;

namespace MyMechanicShop.Catalog;

/* Inherit from this class for your domain layer tests, as abstract generic classes.
 * Close them in the EF Core test project, like ProductRepository_Tests.
 */
public abstract class CatalogDomainTestBase<TStartupModule> : CatalogTestBase<TStartupModule>
    where TStartupModule : IAbpModule;
