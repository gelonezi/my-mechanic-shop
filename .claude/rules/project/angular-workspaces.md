---
paths:
  - "**/angular/**/*.ts"
  - "**/angular/**/*.json"
  - "**/angular/**/*.ps1"
  - "**/public-api.ts"
  - "**/ng-package.json"
---

## Angular: two workspaces, module UI ships as a package

`angular/` and `modules/<Name>/angular/` are **separate npm projects** — own
`package.json`, own `node_modules`, own `angular.json`. There is no Angular equivalent of
a `ProjectReference`: on the .NET side a module compiles *into* the host, but on the
Angular side the module builds an npm package (`@my-mechanic-shop/catalog`) that the host
app consumes.

| | `angular/` | `modules/<Name>/angular/` |
| --- | --- | --- |
| Is | the deployable application | a library + a `dev-app` stub |
| Projects | `MyMechanicShop` | `catalog` (library), `dev-app` (no build target) |
| Output | the site on :4200 | `dist/catalog`, an npm package |

`dev-app` has **no build or serve target** — it exists so `@abp/ng.schematics` has a
`--source` project for proxy generation. It is the seed of a standalone Catalog frontend,
not one today.

Four things wire the library into the host, all of them required:

1. `angular/tsconfig.json` `paths` maps `@my-mechanic-shop/catalog` and
   `@my-mechanic-shop/catalog/*` to `../modules/MyMechanicShop.Catalog/angular/dist/catalog`
   — the **ng-packagr output**, not the source, so the host consumes exactly the package it
   would get from npm. Cost: `yarn ng build catalog` after every library change.
2. `angular/src/environments/environment.ts` *and* `environment.prod.ts` each need a
   `Catalog` entry, because the generated services declare `apiName = 'Catalog'`. **This is
   the microservice seam**: when Catalog becomes its own pod, these urls are what change.
3. `provideCatalog()` from `@my-mechanic-shop/catalog/config` in `app.config.ts`, beside
   ABP's own `provideIdentityConfig()` etc. The `/config` secondary entry point is what
   registers the route with `RoutesService` and puts Catalog in the nav menu; the route in
   `app.routes.ts` only makes the URL work.
4. Anything the host imports must be exported from the library's `public-api.ts`.

`ng generate` in the module workspace needs `--project catalog`: `defaultProject` was
removed in Angular 17 and that workspace has two projects. Angular 20 also dropped the
`.component` file and class suffixes, so the Book Store tutorial's filenames will not match.

### Do not register modules in `angular/scripts/symlink-config.ps1`

`$PackageDirectories` is deliberately empty. Those scripts symlink this app's
`@angular`/`@abp` into a module workspace so both compile against one instance — which
matters when a host compiles library *source*, not when it consumes a built package as we
do. Registering a module there **breaks its build**: Node resolves the junction to its real
path, so `@angular/build` looks for `ng-packagr` in the app's `node_modules` (an
application has no reason to have it) and `ng build <lib>` dies with a silent exit 1.

Both scripts are destructive and not reversible without a reinstall: setup deletes each
original package before junctioning it, and `symlinks:remove` deletes the module's **entire**
`node_modules`. To undo a junction safely, delete the link only — `rmdir` without `/s`, or
`[System.IO.Directory]::Delete(path, $false)` — then `yarn install` in that workspace.

