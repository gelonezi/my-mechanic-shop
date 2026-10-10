import { permissionGuard, RouterOutletComponent } from '@abp/ng.core';
import { Routes } from '@angular/router';

/* Mounted by the host at /catalog (see angular/src/app/app.routes.ts), so every path
 * here is relative: '' -> /catalog, 'products' -> /catalog/products.
 *
 * RouterOutletComponent is ABP's pass-through shell. It gives the module one parent
 * route to hang children off, which is what lets a second page (Stock Control) be added
 * later without the host knowing anything about it. */
export const CATALOG_ROUTES: Routes = [
  {
    path: '',
    component: RouterOutletComponent,
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'products' },
      /* Hiding the menu entry is not access control — the URL can still be typed. The
       * guard is the real gate, and it reads the same policy from `data`.
       *
       * permissionGuard is ABP's functional CanActivateFn (the modern form; the older
       * PermissionGuard class is still exported). It resolves the policy through
       * PermissionService, so it waits for the application-configuration call rather
       * than racing it. The server still enforces the permission on every request —
       * this only stops the user reaching a page that would answer 403. */
      {
        path: 'products',
        canActivate: [permissionGuard],
        data: { requiredPolicy: 'Catalog.Products' },
        loadComponent: () => import('./product/product').then(c => c.Product),
      },
    ],
  },
];
