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
    }

    private static void ConfigureExtraProperties()
    {
    }
}
