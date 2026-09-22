using Volo.Abp.Reflection;

namespace MyMechanicShop.Catalog.Permissions;

public static class CatalogPermissions
{
    public const string GroupName = "Catalog";

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(CatalogPermissions));
    }
}
