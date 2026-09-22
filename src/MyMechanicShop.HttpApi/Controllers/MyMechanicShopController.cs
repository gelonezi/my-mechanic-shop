using MyMechanicShop.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace MyMechanicShop.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class MyMechanicShopController : AbpControllerBase
{
    protected MyMechanicShopController()
    {
        LocalizationResource = typeof(MyMechanicShopResource);
    }
}
