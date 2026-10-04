using Volo.Abp.Threading;

namespace MyMechanicShop.EntityFrameworkCore;

public static class MyMechanicShopEfCoreEntityExtensionMappings
{
    private static readonly OneTimeRunner OneTimeRunner = new OneTimeRunner();

    public static void Configure()
    {
        MyMechanicShopGlobalFeatureConfigurator.Configure();
        MyMechanicShopModuleExtensionConfigurator.Configure();

        OneTimeRunner.Run(() =>
        {
            /* You can configure extra properties for the
             * entities defined in the modules used by your application.
             *
             * This class can be used to map these extra properties to table fields in the database,
             * with ObjectExtensionManager.Instance.MapEfCoreProperty<TEntity, TProperty>(...).
             *
             * USE THIS CLASS ONLY TO CONFIGURE EF CORE RELATED MAPPING.
             * USE MyMechanicShopModuleExtensionConfigurator CLASS (in the Domain.Shared project)
             * FOR A HIGH LEVEL API TO DEFINE EXTRA PROPERTIES TO ENTITIES OF THE USED MODULES
             *
             * See https://abp.io/docs/latest/framework/architecture/modularity/extending/customizing-application-modules-extending-entities
             */
        });
    }
}
