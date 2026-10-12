using Microsoft.Extensions.Localization;
using MyMechanicShop.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace MyMechanicShop;

[Dependency(ReplaceServices = true)]
public class MyMechanicShopBrandingProvider(IStringLocalizer<MyMechanicShopResource> localizer) : DefaultBrandingProvider
{
    public override string AppName => localizer["AppName"];
}
