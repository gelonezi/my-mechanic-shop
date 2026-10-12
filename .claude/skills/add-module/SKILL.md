---
name: add-module
description: Add a new business module (a local ABP module with its own database, migrations, tests and Angular library) to MyMechanicShop, end to end — scaffold with `abp new-module`, strip the ABP Studio metadata, wire it into the host by hand, give it its database, test database, Angular library in the pnpm workspace and just recipes. Use when the user wants a new module or bounded context (e.g. "create a Sales module", "split Inventory into its own module").
---

# Add a module

`Catalog` is the reference: when in doubt, do what it does. Throughout, `<Name>` is the
PascalCase module name (`Sales`) and `<name>` its lower-case form (`sales`). The project
prefix is always `MyMechanicShop.<Name>`.

Work on a branch (`main` is PR-only). Commit after each phase that builds; this is a long
change and a broken intermediate state is easier to bisect in small commits.

## 1. Scaffold

From the repo root:

```
abp new-module MyMechanicShop.<Name> -t ddd -u angular -d ef -o modules/MyMechanicShop.<Name>
```

Verified on ABP Studio CLI 3.1.4: it works without the repo's (removed) `.abpsln` and writes a
standalone module. Its templates reference **ABP 10.6.1** — the steps below bring it to the
solution's version.

## 2. Strip what this repo removed

Delete, inside `modules/MyMechanicShop.<Name>/`:

- `*.abpsln`, `*.abpmdl`, every `*.abppkg` and `*.abppkg.analyze.json`, `.abpignore`
- `.abpstudio/` and `.cursor/` — ABP's current AI rules (`*.mdc`). Before deleting, a quick
  diff against `.claude/rules/framework/` and `template/` can surface new ABP guidance worth
  porting; the rules here are maintained by hand now.
- `.editorconfig`, `angular/.editorconfig`, `.gitignore`, `angular/.gitignore`
  (`project/repo-config-files.md`: the Angular `.editorconfig` has `root = true` and would cut
  the module off from the root file), `angular/web.config`, any `.DS_Store`
- the template's `Samples/` code and its sample tests (Catalog keeps none)

## 3. .NET projects

- Replace `modules/MyMechanicShop.<Name>/common.props` with a copy of Catalog's.
- In every `.csproj`: remove each `Version="…"` (NU1008 otherwise), `<TargetFramework>` and
  `<Nullable>`. A package that `Directory.Packages.props` lacks fails with NU1010: add its
  `PackageVersion` there (ABP packages at the solution's version, not 10.6.1).
- Application services are not `sealed`, and their public/protected methods are `virtual`
  (`template/module.md`; ABP's Castle proxies subclass them).
- If the module uses value objects or shared enums, link the Shared Kernel
  (`project/shared-kernel.md`, two `ProjectReference`s + `[DependsOn]`).
- Add the projects to the module's own `.slnx` (already there) **and** to the root
  `MyMechanicShop.slnx` under `/modules/MyMechanicShop.<Name>/src/` and `/test/`.

Check: `dotnet build modules/MyMechanicShop.<Name>/MyMechanicShop.<Name>.slnx`.

## 4. Wire into the host

By hand, layer to layer: seven `ProjectReference`s and seven `[DependsOn]`s — the table in
`project/solution-and-modules.md`. Never reference the module's test projects from host test
projects (`project/testing.md`).

Shape the API in `<Name>HttpApiModule.PreConfigureServices` like Catalog's: `RootPath`,
`RemoteServiceName` from `<Name>RemoteServiceConsts`, and a `UrlControllerNameNormalizer` arm
per entity app service (`project/auto-api-controllers.md`).

## 5. Its own database

Follow "Adding a module database" in `project/databases-and-migrations.md`. In short:

1. `ConnectionStrings:<Name>` (`Database=MyMechanicShop<Name>`, `Host=127.0.0.1`, no password) in
   **both** `src/MyMechanicShop.DbMigrator/appsettings.json` and
   `src/MyMechanicShop.HttpApi.Host/appsettings.json`; `<Name>DbProperties.ConnectionStringName`
   = `"<Name>"`.
2. The host's `MyMechanicShopDbContext` must **not** replace, implement or configure the
   module's context.
3. Module EF project: `Volo.Abp.EntityFrameworkCore.PostgreSql` + `Microsoft.EntityFrameworkCore.Design`
   (copy Catalog's `PrivateAssets`), and a design-time factory copied from
   `CatalogDbContextFactory` — environment-only configuration, **no** `MigrationsAssembly`.
4. `Data/I<Name>DbSchemaMigrator` + `Data/Null<Name>DbSchemaMigrator` in `<Name>.Domain`,
   `EntityFrameworkCore<Name>DbSchemaMigrator` in the EF project — names ending in
   `<Name>DbSchemaMigrator`, so ABP exposes them without `[ExposeServices]`.
5. Inject `I<Name>DbSchemaMigrator` into `MyMechanicShopDbMigrationService` and call it in
   `MigrateDatabaseSchemaAsync`. **Forgetting this is silent**: the migrator still logs
   success and the database stays empty.
6. Initial migration: the `ef-migration` skill.

## 6. Tests

- Host harness: `MyMechanicShopEntityFrameworkCoreTestModule` gets the module's own
  `AbpUnitTestSqliteDatabase`, `CreateTables` for its context, the mapping to its
  `ConnectionStringName`, and disposal in `OnApplicationShutdown` (`project/testing.md`).
- Module tests stay in the module and run from its `.slnx`. Abstract generic tests live in
  `Domain.Tests` / `Application.Tests`; `EntityFrameworkCore.Tests` closes them.

## 7. Angular library

`modules/*/angular` is already in `pnpm-workspace.yaml`, so the new workspace joins on the
next install. Then fix what the template got wrong for this repo
(`project/angular-workspaces.md`):

- `angular/package.json`: every `@abp/*` and `@volo/*` range from `~10.6.1` to the
  solution's (`~10.7.0`), matching Catalog's.
- `$schema` paths are root-relative: `../../../node_modules/…` in `angular/angular.json`,
  `../../../../../node_modules/…` in `projects/<name>/ng-package.json` — there is one
  `node_modules`, at the repo root.
- `projects/<name>/ng-package.json`: add `"deleteDestPath": false`.
- `pnpm install` at the root (not frozen: the lockfile gains the new importer). If pnpm stops
  on `ERR_PNPM_IGNORED_BUILDS`, decide with `pnpm approve-builds`.
- Host wiring, all four: `paths` for `@my-mechanic-shop/<name>` (+ `/*`) to its `dist/<name>`
  in `angular/tsconfig.json`; a `<Name>` entry in both `environment.ts` and
  `environment.prod.ts`; `provide<Name>()` from `@my-mechanic-shop/<name>/config` in
  `app.config.ts`; exports in the library's `public-api.ts`.
- Never register the module in `angular/scripts/symlink-config.ps1` (silent build failure).

## 8. Recipes

In the root `justfile`, mirroring Catalog's lines: a path variable, a `<name>-build` recipe,
a private `<name>-watch`, the build in `web`'s dependencies **before** `web-dev`, the watcher
in `web-dev`'s `[parallel]` list, and a `proxies` line for the module
(`-m <name> -s dev-app --target <name> -a <Name>`, run in its `angular/` folder) plus the LF
rewrite for its `proxy/` folder.

## 9. Verify

1. `dotnet build MyMechanicShop.slnx` and `dotnet test MyMechanicShop.slnx`; both module
   `.slnx` files.
2. From empty databases — `docker compose -f etc/docker/infrastructure/postgresql.yml down -v`
   **deletes all local data: ask first** — then `just api`, then:

   ```
   docker exec mymechanicshop-postgresql psql -U postgres -d MyMechanicShop<Name> -c '\dt'
   ```

   No tables means the migrator never ran its migrations (step 5).
3. `just proxies` with the API up; `just web`, open http://localhost:4200, find the menu entry.
4. Update the databases table in `project/databases-and-migrations.md`, the test count in
   `CLAUDE.md` / `project/testing.md`, and the README's solution structure. The rules' `**/` globs
   already cover `modules/**`; no rule needs new paths.
