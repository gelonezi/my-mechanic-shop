import { RouterOutletComponent } from '@abp/ng.core';
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
      {
        path: 'products',
        loadComponent: () => import('./product/product').then(c => c.Product),
      },
    ],
  },
];
