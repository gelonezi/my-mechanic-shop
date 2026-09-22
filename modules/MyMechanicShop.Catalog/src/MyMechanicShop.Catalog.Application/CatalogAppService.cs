using MyMechanicShop.Catalog.Localization;
using Volo.Abp.Application.Services;

namespace MyMechanicShop.Catalog;

public abstract class CatalogAppService : ApplicationService
{
    protected CatalogAppService()
    {
        LocalizationResource = typeof(CatalogResource);
        ObjectMapperContext = typeof(CatalogApplicationModule);
    }
}
