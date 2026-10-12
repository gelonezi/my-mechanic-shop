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
        // ABP extension point: change max lengths of the used modules' entity properties here. Nothing to change yet.
    }

    private static void ConfigureExtraProperties()
    {
        // ABP extension point: add extra properties to the used modules' entities here (ObjectExtensionManager). None yet.
    }
}
