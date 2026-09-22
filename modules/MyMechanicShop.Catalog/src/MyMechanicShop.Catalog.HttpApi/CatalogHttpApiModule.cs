using Localization.Resources.AbpUi;
using MyMechanicShop.Catalog.Localization;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace MyMechanicShop.Catalog;

[DependsOn(typeof(CatalogApplicationModule))]
public class CatalogHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<AbpAspNetCoreMvcOptions>(options =>
        {
            options
                .ConventionalControllers
                .Create(typeof(CatalogApplicationModule).Assembly, opts =>
                {
                    opts.RootPath = CatalogRemoteServiceConsts.ModuleName;
                    opts.RemoteServiceName = CatalogRemoteServiceConsts.RemoteServiceName;
                    opts.UrlControllerNameNormalizer = ctx =>
                        ctx.ControllerName == "Product" ? "products" : ctx.ControllerName;
                });
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<CatalogResource>()
                .AddBaseTypes(typeof(AbpUiResource));
        });
    }
}
