using Volo.Abp.Modularity;
using Volo.Abp.Domain;
using Volo.Abp.Validation;
using Volo.Abp.VirtualFileSystem;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Localization;
using MyMechanicShop.SharedKernel.Localization;
using Volo.Abp.Validation.Localization;

namespace MyMechanicShop.SharedKernel;

[DependsOn(
    typeof(AbpValidationModule),
    typeof(AbpDddDomainSharedModule)
)]
public class SharedKernelDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SharedKernelDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<SharedKernelResource>("en")
                .AddBaseTypes(typeof(AbpValidationResource))
                .AddVirtualJson("/Localization/SharedKernel");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("SharedKernel", typeof(SharedKernelResource));
        });
    }
}
