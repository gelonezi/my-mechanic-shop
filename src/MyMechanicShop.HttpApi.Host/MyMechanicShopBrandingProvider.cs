using Microsoft.Extensions.Localization;
using MyMechanicShop.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace MyMechanicShop;

[Dependency(ReplaceServices = true)]
public class MyMechanicShopBrandingProvider : DefaultBrandingProvider
{
    private readonly IStringLocalizer<MyMechanicShopResource> _localizer;

    public MyMechanicShopBrandingProvider(IStringLocalizer<MyMechanicShopResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
