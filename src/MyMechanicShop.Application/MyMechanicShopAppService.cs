using MyMechanicShop.Localization;
using Volo.Abp.Application.Services;

namespace MyMechanicShop;

/* Inherit your application services from this class.
 */
public abstract class MyMechanicShopAppService : ApplicationService
{
    protected MyMechanicShopAppService()
    {
        LocalizationResource = typeof(MyMechanicShopResource);
    }
}
