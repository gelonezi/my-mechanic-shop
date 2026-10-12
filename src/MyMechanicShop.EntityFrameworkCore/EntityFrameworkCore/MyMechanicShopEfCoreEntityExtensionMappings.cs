using Volo.Abp.Threading;

namespace MyMechanicShop.EntityFrameworkCore;

public static class MyMechanicShopEfCoreEntityExtensionMappings
{
    private static readonly OneTimeRunner OneTimeRunner = new OneTimeRunner();

    public static void Configure()
    {
        MyMechanicShopGlobalFeatureConfigurator.Configure();
        MyMechanicShopModuleExtensionConfigurator.Configure();

        OneTimeRunner.Run(() => { });
    }
}
