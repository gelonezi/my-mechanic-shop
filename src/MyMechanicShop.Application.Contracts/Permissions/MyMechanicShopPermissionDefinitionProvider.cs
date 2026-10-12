using MyMechanicShop.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace MyMechanicShop.Permissions;

public class MyMechanicShopPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        context.AddGroup(MyMechanicShopPermissions.GroupName);
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MyMechanicShopResource>(name);
    }
}
