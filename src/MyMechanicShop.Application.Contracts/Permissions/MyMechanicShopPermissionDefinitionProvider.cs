using MyMechanicShop.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace MyMechanicShop.Permissions;

public class MyMechanicShopPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(MyMechanicShopPermissions.GroupName);

        //Define your own permissions here. Example:
        //myGroup.AddPermission(MyMechanicShopPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MyMechanicShopResource>(name);
    }
}
