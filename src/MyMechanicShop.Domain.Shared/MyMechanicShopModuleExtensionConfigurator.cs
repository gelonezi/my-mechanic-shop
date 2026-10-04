using Volo.Abp.Threading;

namespace MyMechanicShop;

public static class MyMechanicShopModuleExtensionConfigurator
{
    private static readonly OneTimeRunner OneTimeRunner = new OneTimeRunner();

    public static void Configure()
    {
        OneTimeRunner.Run(() =>
        {
            ConfigureExistingProperties();
            ConfigureExtraProperties();
        });
    }

    private static void ConfigureExistingProperties()
    {
        /* You can change max lengths for properties of the
         * entities defined in the modules used by your application.
         *
         * For example, the user and role name max lengths live in
         * AbpUserConsts.MaxNameLength and IdentityRoleConsts.MaxNameLength.
         *
         * Notice: It is not suggested to change property lengths
         * unless you really need it. Go with the standard values wherever possible.
         *
         * If you are using EF Core, you will need to run the add-migration command after your changes.
         */
    }

    private static void ConfigureExtraProperties()
    {
        /* You can configure extra properties for the
         * entities defined in the modules used by your application.
         *
         * This class can be used to define these extra properties
         * with a high level, easy to use API.
         *
         * For example, ObjectExtensionManager.Instance.Modules().ConfigureIdentity(...)
         * adds a property to the Identity module's user entity.
         *
         * See the documentation for more:
         * https://abp.io/docs/latest/framework/architecture/modularity/extending/module-entity-extensions
         */
    }
}
