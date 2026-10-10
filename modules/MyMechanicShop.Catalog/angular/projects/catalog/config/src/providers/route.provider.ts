import { eLayoutType, RoutesService } from '@abp/ng.core';
import {
  EnvironmentProviders,
  inject,
  makeEnvironmentProviders,
  provideAppInitializer,
} from '@angular/core';
import { eCatalogRouteNames } from '../enums/route-names';

export const CATALOG_ROUTE_PROVIDERS = [
  provideAppInitializer(() => {
    configureRoutes();
  }),
];

/* Adds the module's entries to the host's menu. This is the /config secondary entry
 * point, imported by the host as @my-mechanic-shop/catalog/config — deliberately
 * separate from the main entry point so the host can register the menu without pulling
 * in the page components. */
export function configureRoutes() {
  const routesService = inject(RoutesService);
  routesService.add([
    {
      path: '/catalog',
      name: eCatalogRouteNames.Catalog,
      iconClass: 'fas fa-boxes-stacked',
      layout: eLayoutType.application,
      order: 3,
    },
    /* parentName nests this under the Catalog entry, so the menu reads
     * Catalog > Products. Stock Control becomes a second child later.
     *
     * requiredPolicy hides the entry unless the permission is granted — RoutesService
     * filters on it, so this is declarative, not an *ngIf. It belongs on the CHILD and
     * deliberately not on the Catalog parent above: Catalog.Products is a Host-side
     * permission (MultiTenancySides.Host), so a tenant user can never hold it. Gating
     * the parent on it would hide the whole Catalog menu from every tenant — including
     * the My Store page that comes next, which is Tenant-side. */
    {
      path: '/catalog/products',
      name: eCatalogRouteNames.Products,
      parentName: eCatalogRouteNames.Catalog,
      requiredPolicy: 'Catalog.Products',
      layout: eLayoutType.application,
      order: 1,
    },
  ]);
}

const CATALOG_PROVIDERS: EnvironmentProviders[] = [...CATALOG_ROUTE_PROVIDERS];

export function provideCatalog() {
  return makeEnvironmentProviders(CATALOG_PROVIDERS);
}
