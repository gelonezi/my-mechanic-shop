---
name: ef-migration
description: Add, apply, list or remove an EF Core migration in MyMechanicShop — for the host's MyMechanicShopDbContext or a module's own DbContext — and verify it reached the right PostgreSQL database. Use when an entity or EF configuration changed, when the user asks how to create or apply a migration, or when a database looks out of date or empty.
---

# EF Core migrations

Every DbContext keeps its migrations **next to itself**, and `src/MyMechanicShop.DbMigrator`
applies all of them (`project/databases-and-migrations.md`). Generating a migration writes
code: say which command you are about to run, or give the user the commands if they asked
how to do it.

Prerequisite: the `dotnet-ef` global tool (`dotnet tool install -g dotnet-ef`, matching the
EF Core version in `Directory.Packages.props`). Stop a running API first (`just api`): it
locks `bin/` and the build inside `dotnet ef` fails with MSB3021, which is a file lock, not a
compile error.

## Which context, which folder

| Context | Run `dotnet ef` from | Add |
| --- | --- | --- |
| `MyMechanicShopDbContext` (host: ABP's own tables) | `src/MyMechanicShop.EntityFrameworkCore` | `dotnet ef migrations add <Name> --context MyMechanicShopDbContext --output-dir Migrations/ABP` |
| `<Module>DbContext` (e.g. `CatalogDbContext`) | `modules/MyMechanicShop.<Module>/src/MyMechanicShop.<Module>.EntityFrameworkCore` | `dotnet ef migrations add <Name> --output-dir Migrations` |

- Names read as what changed: `Added_StoreProducts`, `Initial`.
- The **host's** design-time factory reads `../MyMechanicShop.DbMigrator/appsettings.json`
  relative to the current directory, so run it from exactly that folder.
- A **module's** factory reads only the environment (a module can't know where the host
  lives). `migrations add` and `has-pending-model-changes` need no database. Commands that
  query one (`migrations list`, `migrations remove` without `--force`) need
  `ConnectionStrings__<Module>` set, e.g. for Catalog:
  `Host=127.0.0.1;Port=5432;Database=MyMechanicShopCatalog;User ID=postgres;`
- Never set `MigrationsAssembly` (host module, factories): EF's default — the context's own
  assembly — is what makes each module own its migrations.

## Check before applying

```
dotnet ef migrations has-pending-model-changes            # module: no --context needed
dotnet ef migrations has-pending-model-changes --context MyMechanicShopDbContext   # host
```

"No changes have been made to the model since the last migration" after `add` means the
snapshot matches the model. Read the generated `Up`/`Down` before applying; renames often
come out as drop + add.

## Apply

- **Development:** `just api`. The host runs the DbMigrator's migration service before it
  listens: host database, then every module's `I<Module>DbSchemaMigrator`, then the seed.
- **Elsewhere, or without starting the API:** from `src/MyMechanicShop.DbMigrator`,
  `dotnet run`. Run it **from that folder**: it is the content root, so
  `dotnet run --project src/MyMechanicShop.DbMigrator` finds no `appsettings.json` and no
  connection strings. PostgreSQL must be up (`just infra`).

## Verify it reached the right database

The migrator logs *"Successfully completed all database migrations"* even when a module's
database got nothing, so check the tables, not the log:

```
docker exec mymechanicshop-postgresql psql -U postgres -d MyMechanicShopCatalog -c '\dt'
docker exec mymechanicshop-postgresql psql -U postgres -d MyMechanicShopCatalog -c 'select "MigrationId" from "__EFMigrationsHistory"'
```

Each database's history must list only its own context's migrations. A module database with
no tables means `MyMechanicShopDbMigrationService` does not call that module's schema
migrator, or a `MigrationsAssembly` points somewhere else.

In a database client, check which database the connection actually opens: a data source
named after the Catalog database but set to `MyMechanicShop` shows the host's 41 tables and
looks exactly like "the migration went to the wrong database".

## Remove or reset

- Last migration, not yet applied: `dotnet ef migrations remove` from the same folder (a
  module needs `ConnectionStrings__<Module>`, or `--force` when you know it's unapplied).
- Already applied locally: databases are disposable.
  `docker compose -f etc/docker/infrastructure/postgresql.yml down -v` deletes **all** local
  data — confirm with the user — and the next `just api` recreates and reseeds everything.
