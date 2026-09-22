using Volo.Abp.Modularity;

namespace MyMechanicShop.Catalog;

/* Inherit from this class for your application layer tests.
 * See SampleAppService_Tests for example.
 */
public abstract class CatalogApplicationTestBase<TStartupModule> : CatalogTestBase<TStartupModule>
    where TStartupModule : IAbpModule;
