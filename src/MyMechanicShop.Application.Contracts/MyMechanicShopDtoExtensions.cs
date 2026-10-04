using Volo.Abp.Threading;

namespace MyMechanicShop;

public static class MyMechanicShopDtoExtensions
{
    private static readonly OneTimeRunner OneTimeRunner = new OneTimeRunner();

    public static void Configure()
    {
        OneTimeRunner.Run(() =>
        {
            /* You can add extension properties to DTOs
             * defined in the depended modules, with
             * ObjectExtensionManager.Instance.AddOrUpdateProperty<TDto, TProperty>(...).
             * See https://abp.io/docs/latest/framework/fundamentals/object-extensions
             */
        });
    }
}
