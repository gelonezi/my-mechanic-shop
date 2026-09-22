using MyMechanicShop.Catalog.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace MyMechanicShop.Catalog;

public abstract class CatalogController : AbpControllerBase
{
    protected CatalogController()
    {
        LocalizationResource = typeof(CatalogResource);
    }
}
