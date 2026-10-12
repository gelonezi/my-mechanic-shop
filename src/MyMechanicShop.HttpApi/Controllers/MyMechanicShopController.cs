using MyMechanicShop.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace MyMechanicShop.Controllers;

public abstract class MyMechanicShopController : AbpControllerBase
{
    protected MyMechanicShopController()
    {
        LocalizationResource = typeof(MyMechanicShopResource);
    }
}
