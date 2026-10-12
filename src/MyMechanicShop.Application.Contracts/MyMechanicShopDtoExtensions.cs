using Volo.Abp.Threading;

namespace MyMechanicShop;

public static class MyMechanicShopDtoExtensions
{
    private static readonly OneTimeRunner OneTimeRunner = new OneTimeRunner();

    public static void Configure()
    {
        OneTimeRunner.Run(() => { });
    }
}
