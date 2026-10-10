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
                    // Singular Swagger tag (from the class name), plural URL — ABP's own convention.
                    // Return PascalCase: ABP kebab-cases it for the URL (store-products), but
                    // reports it verbatim to generate-proxy, which names the service class from it.
                    opts.UrlControllerNameNormalizer = ctx => ctx.ControllerName switch
                    {
                        "Product" => "Products",
                        "StoreProduct" => "StoreProducts",
                        _ => ctx.ControllerName,
                    };
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
