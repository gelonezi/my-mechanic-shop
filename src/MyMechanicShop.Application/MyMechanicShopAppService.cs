using MyMechanicShop.Localization;
using Volo.Abp.Application.Services;

namespace MyMechanicShop;

public abstract class MyMechanicShopAppService : ApplicationService
{
    protected MyMechanicShopAppService()
    {
        LocalizationResource = typeof(MyMechanicShopResource);
    }
}
