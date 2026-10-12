using Volo.Abp.Reflection;

namespace MyMechanicShop.Catalog.Permissions;

public static class CatalogPermissions
{
    public const string GroupName = "Catalog";

    public static class Products
    {
        public const string Default = GroupName + ".Products";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class StoreProducts
    {
        public const string Default = GroupName + ".StoreProducts";
        public const string Activate = Default + ".Activate";
        public const string Edit = Default + ".Edit";
        public const string Deactivate = Default + ".Deactivate";
    }

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(CatalogPermissions));
    }
}
