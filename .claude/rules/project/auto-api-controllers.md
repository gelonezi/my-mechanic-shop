---
paths:
  - "**/*HttpApiModule*.cs"
  - "**/*AppService*.cs"
  - "**/*RemoteServiceConsts*.cs"
  - "**/proxy/**/*.ts"
  - "**/*.abprun.json"
---

## Auto API controller naming (Catalog)

`CatalogHttpApiModule.PreConfigureServices` shapes the module's whole API surface. All
four settings matter; `RootPath` and `RemoteServiceName` put the two `CatalogRemoteServiceConsts`
to work — before, both were dead and the module published under the generic `app` root path
while `CatalogHttpApiClientModule` registered its proxies under `"Catalog"`.

Current result:

| App service | Swagger tag | Route |
| --- | --- | --- |
| `ProductAppService` | **`Product`** | **`/api/catalog/products`** |
| `StoreProductAppService` | **`StoreProduct`** | **`/api/catalog/store-products`** |

That split is deliberate and follows ABP's own rule, which holds with no counterexamples in
this solution: **entity CRUD controllers get a singular tag and a plural URL**
(`User` → `/api/identity/users`, `Role` → `/api/identity/roles`, `Tenant` →
`/api/multi-tenancy/tenants`). ABP's plural tags — `Permissions`, `Features`,
`EmailSettings`, `TimeZoneSettings`, `DynamicClaims` — are settings bundles, not entity
collections, so they are not counterexamples.

The class name drives the tag; `UrlControllerNameNormalizer` rewrites only the URL:

```csharp
opts.UrlControllerNameNormalizer = ctx => ctx.ControllerName switch
{
    "Product" => "Products",
    "StoreProduct" => "StoreProducts",
    _ => ctx.ControllerName,
};
```

Every new entity app service needs its arm; without one, its URL falls back to the singular
name. **Return the plural in PascalCase, never kebab-case.** ABP uses the returned value twice
(verified in the `rel-10.7` source):

- `ConventionalRouteBuilder` kebab-cases it for the URL — `StoreProducts` → `store-products`.
- `AspNetCoreApiDescriptionModelProviderOptions.ControllerNameGenerator` reports it **verbatim**
  as `controllerName` in `/api/abp/api-definition`, and `abp generate-proxy` names the Angular
  service class `<controllerName>Service`. Returning `"store-products"` made it emit
  `export class store-productsService` — not valid TypeScript, so `ng build catalog` failed —
  and `"products"` gave the lower-case `productsService`.

Two traps, both found the hard way by diffing the live `/swagger/v1/swagger.json`:

- `UrlControllerNameNormalizer` rewrites the URL **only while nothing else renames the
  controller**. Add a `ControllerModelConfigurer` and it runs *first*, so `ctx.ControllerName`
  arrives already renamed and the normalizer's match has to test the *new* name. Getting that
  backwards silently moves the tag, the URL, or both. Renaming the app service class is the
  simpler lever whenever it is available — reach for the normalizer only to split tag from URL.
- `/api/abp/api-definition` reports `controllerName` as the normalizer's output (`Products`),
  not the tag. `abp generate-proxy` reads that file, so generated client and Angular services
  are named from it (`ProductsService`) and will not match the singular Swagger heading.
  Expected, not a bug.

Catalog's Angular proxies live in the **module's own library**, not the host app, so the
library ships self-contained. Run from `modules/MyMechanicShop.Catalog/angular` with the
host up:

```
abp generate-proxy -t ng -m catalog -s dev-app --target catalog -a Catalog
```

Every flag is load-bearing: `-m` defaults to `app` and silently skips the module;
`-s dev-app` is the project whose `environment.ts` supplies the API URL; `--target catalog`
puts the output in the library rather than in `dev-app`; `-a Catalog` sets the `apiName` the
generated services declare, which must match the `Catalog` entry in *both* environment files.

`abp run` runs exactly that command on every start, as the `MyMechanicShop.Catalog.Proxies`
application in `Default.abprun.json` → `etc/scripts/generate-catalog-proxies.ps1` (also fine to
run by hand). It is an **application**, not a workflow step, for two verified reasons:

- `abp run` starts `applications` only and ignores `tasks` and `workflows`, so a `RunTask`
  never fires. Applications all start at once, which is why the script polls
  `/health-status` before generating.
- The workflow's `GenerateAngularProxies` action only carries url/module/service type/working
  directory — no `-s`, `--target` or `-a` — so it would generate into `dev-app` with the default
  `apiName` anyway.

The generator deletes and rewrites one folder per controller (`proxy/products/`,
`proxy/store-products/`), plus one per namespace of the enums the DTOs use — `ProductUnit` and
`Currency` come from the Shared Kernel — and writes some files CRLF, some LF. The
script normalizes the folder to LF afterwards and `.gitattributes` pins `**/proxy/**` to
`eol=lf`, so an unchanged API leaves `git status` clean; a real API change is a real diff.
The library watcher picks the rewrite up and `ng serve` reloads.

Keep the urls in `dev-app/src/environments/environment.ts` as plain string literals. The
schematic parses that file statically, so a `const` indirection fails with
`Cannot resolve API URL for "Catalog" remote service name`.
