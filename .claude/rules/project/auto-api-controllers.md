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

Current result: `ProductAppService` → Swagger tag **`Product`**, route
**`/api/catalog/products`**.

That split is deliberate and follows ABP's own rule, which holds with no counterexamples in
this solution: **entity CRUD controllers get a singular tag and a plural URL**
(`User` → `/api/identity/users`, `Role` → `/api/identity/roles`, `Tenant` →
`/api/multi-tenancy/tenants`). ABP's plural tags — `Permissions`, `Features`,
`EmailSettings`, `TimeZoneSettings`, `DynamicClaims` — are settings bundles, not entity
collections, so they are not counterexamples.

The class name drives the tag; `UrlControllerNameNormalizer` rewrites only the URL:

```csharp
opts.UrlControllerNameNormalizer = ctx =>
    ctx.ControllerName == "Product" ? "products" : ctx.ControllerName;
```

Two traps, both found the hard way by diffing the live `/swagger/v1/swagger.json`:

- `UrlControllerNameNormalizer` rewrites the URL **only while nothing else renames the
  controller**. Add a `ControllerModelConfigurer` and it runs *first*, so `ctx.ControllerName`
  arrives already renamed and the normalizer's match has to test the *new* name. Getting that
  backwards silently moves the tag, the URL, or both. Renaming the app service class is the
  simpler lever whenever it is available — reach for the normalizer only to split tag from URL.
- `/api/abp/api-definition` reports `controllerName` as the *URL* segment (`products`), not the
  tag. `abp generate-proxy` reads that file, so generated client and Angular services are named
  from the URL, and will not match the singular Swagger heading. Expected, not a bug.

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

`Default.abprun.json` deliberately has **no** `GenerateAngularProxies` action for Catalog —
that action type only carries url/module/workingDirectory and cannot express `--target`, so
it would generate into the wrong project. The host's own `app` proxies still run from it.

Keep the urls in `dev-app/src/environments/environment.ts` as plain string literals. The
schematic parses that file statically, so a `const` indirection fails with
`Cannot resolve API URL for "Catalog" remote service name`.
