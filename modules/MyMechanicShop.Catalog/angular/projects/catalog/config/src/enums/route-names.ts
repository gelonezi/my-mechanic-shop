/* Values are ABP localization keys, not display text: "<Resource>::<Key>".
 * The resource is Catalog (CatalogResource carries [LocalizationResourceName("Catalog")])
 * and the keys live in
 * modules/MyMechanicShop.Catalog/src/.../Domain.Shared/Localization/Catalog/*.json.
 * If a key is missing, ABP renders the raw string instead of failing. */
export const enum eCatalogRouteNames {
  Catalog = 'Catalog::Menu:Catalog',
  Products = 'Catalog::Menu:Products',
}
