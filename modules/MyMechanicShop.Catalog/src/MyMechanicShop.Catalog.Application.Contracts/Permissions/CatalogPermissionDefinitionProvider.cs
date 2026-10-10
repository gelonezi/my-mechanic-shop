using MyMechanicShop.Catalog.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace MyMechanicShop.Catalog.Permissions;

public class CatalogPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var catalog = context.AddGroup(CatalogPermissions.GroupName, L("Permission:Catalog"));

        // Host only: the global catalog is maintained by the system administrator. A permission
        // defined for the host side does not exist for tenants — their admins cannot see or grant it.
        var products = catalog.AddPermission(
            CatalogPermissions.Products.Default, L("Permission:Products"), MultiTenancySides.Host);
        products.AddChild(CatalogPermissions.Products.Create, L("Permission:Products.Create"), MultiTenancySides.Host);
        products.AddChild(CatalogPermissions.Products.Edit, L("Permission:Products.Edit"), MultiTenancySides.Host);
        products.AddChild(CatalogPermissions.Products.Delete, L("Permission:Products.Delete"), MultiTenancySides.Host);

        // Tenant only: each shop activates catalog products and sets its own prices.
        var storeProducts = catalog.AddPermission(
            CatalogPermissions.StoreProducts.Default, L("Permission:StoreProducts"), MultiTenancySides.Tenant);
        storeProducts.AddChild(CatalogPermissions.StoreProducts.Activate, L("Permission:StoreProducts.Activate"), MultiTenancySides.Tenant);
        storeProducts.AddChild(CatalogPermissions.StoreProducts.Edit, L("Permission:StoreProducts.Edit"), MultiTenancySides.Tenant);
        storeProducts.AddChild(CatalogPermissions.StoreProducts.Deactivate, L("Permission:StoreProducts.Deactivate"), MultiTenancySides.Tenant);
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<CatalogResource>(name);
    }
}
