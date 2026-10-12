---
paths:
  - "**/angular/**/*.ts"
  - "**/angular/**/*.json"
  - "**/angular/**/*.ps1"
  - "**/public-api.ts"
  - "**/ng-package.json"
  - "**/pnpm-workspace.yaml"
---

## Angular: two workspaces, module UI ships as a package

`angular/` and `modules/<Name>/angular/` are **separate projects**, each with its own
`package.json` and `angular.json`, but they are installed together as **one pnpm workspace**
(see below). There is no Angular equivalent of
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
   would get from npm. Cost: the library must be rebuilt after every change. `just web`
   pays it: it builds the library once, then runs `ng build catalog --watch` (rewrites
   `dist/` on every source change, regenerated proxies included) beside `ng serve`, which
   recompiles from there (verified: a template edit is served within ~1 s). `just
   catalog-build` builds it once without serving.

   **`"deleteDestPath": false` in the library's `ng-package.json` is load-bearing.** By
   default ng-packagr empties `dist/` at the start of every build. A watcher and `ng serve`
   started at the same moment then has `ng serve` compile against the empty folder and fail
   with `TS2307: Cannot find module '@my-mechanic-shop/catalog'`. It stays failed after
   `dist/` is rewritten, because it caches the failed resolution. That is why `just web`
   runs `catalog-build` to completion **before** starting the watcher and `ng serve`
   together; on a fresh clone, where `dist/` does not exist yet, that first build is what
   creates it.
2. `angular/src/environments/environment.ts` *and* `environment.prod.ts` each need a
   `Catalog` entry, because the generated services declare `apiName = 'Catalog'`. **This is
   the microservice seam**: when Catalog becomes its own pod, these urls are what change.
3. `provideCatalog()` from `@my-mechanic-shop/catalog/config` in `app.config.ts`, beside
   ABP's own `provideIdentityConfig()` etc. The `/config` secondary entry point is what
   registers the route with `RoutesService` and puts Catalog in the nav menu; the route in
   `app.routes.ts` only makes the URL work.
4. Anything the host imports must be exported from the library's `public-api.ts`.

### One pnpm workspace, one hoisted `node_modules` at the root

The root `package.json` pins pnpm (`packageManager`), `pnpm-workspace.yaml` lists
`angular` and `modules/*/angular`, and there is a single `pnpm-lock.yaml`. It was produced
by `pnpm import` from the two former `yarn.lock` files (0 conflicting ranges), so the
versions are the ones yarn had. Run workspace commands with a path filter:
`pnpm --filter ./angular exec ng …`. Never `yarn` or `npm install` in a workspace folder.

- **`nodeLinker: hoisted`**: a flat `node_modules`, the layout yarn had. pnpm's default
  isolated layout breaks the Angular CLI builders and ABP's schematics, which load
  packages they don't declare. `abp generate-proxy` was verified on this layout.
- **There is one `node_modules`, at the repo root**; neither workspace folder has its own.
  So every `node_modules/…` path in an `angular.json` or `ng-package.json` is written
  relative to the root: `../node_modules/…` from `angular/`, `../../../node_modules/…` from
  `modules/<Name>/angular/`. The app's global styles come from packages the app does not
  even declare (`@fortawesome/fontawesome-free`, `ng-zorro-antd`, LeptonX), so no pnpm
  layout can put them under `angular/node_modules`. A tool that writes `node_modules/…`
  back into `angular.json` (an `ng add`, an ABP update) breaks the build with
  `Could not resolve "node_modules/…"`. Fix the path, not the layout.
- **Dependency install scripts are off** (`allowBuilds: … false` for `esbuild`,
  `@parcel/watcher`, `lmdb`, `msgpackr-extract`), as `angular/Dockerfile` already did with
  `--ignore-scripts`. Each ships prebuilt binaries as optional packages. pnpm refuses to
  install (`ERR_PNPM_IGNORED_BUILDS`) when a new dependency brings an install script:
  decide with `pnpm approve-builds '<pkg>'` or `'!<pkg>'`, which records it in the file.

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
`[System.IO.Directory]::Delete(path, $false)` — then `just web-install`.

