---
paths:
  - "**/*.EntityFrameworkCore/**/*.cs"
  - "**/*DbContext*.cs"
  - "**/Migrations/**/*.cs"
  - "**/*DbMigrator*/**"
  - "**/appsettings*.json"
  - "**/*DbSchemaMigrator*.cs"
---

## Databases and migrations

Target architecture: **one database per module**. **Each module creates its own
migrations**, in its own `*.EntityFrameworkCore` project, next to its DbContext.
**`src/MyMechanicShop.DbMigrator` executes them all**: point it at an environment and its
connection strings bring every database up to date. So module EF projects are **not**
provider-agnostic. They reference `Volo.Abp.EntityFrameworkCore.PostgreSql` and
`Microsoft.EntityFrameworkCore.Design`, and own a `Migrations/` folder.

The provider is **PostgreSQL**, one server holding one database per module:

| Database | Connection string | DbContext | Migrations |
| --- | --- | --- | --- |
| `MyMechanicShop` | `Default` | `MyMechanicShopDbContext` | `src/MyMechanicShop.EntityFrameworkCore/Migrations/ABP/` |
| `MyMechanicShopCatalog` | `Catalog` | `CatalogDbContext` | `modules/MyMechanicShop.Catalog/src/MyMechanicShop.Catalog.EntityFrameworkCore/Migrations/` |

Locally the server is `etc/docker/infrastructure/postgresql.yml` (`postgres:18`, the
current major; pinned to the major so a minor update never needs `pg_upgrade`). `just infra`
starts it (`docker compose … up -d --wait`), and `just api` runs `infra` first. `--wait`
returns once the container is healthy, ~0.2 s after start (~2 s on a first
`initdb`). It keeps running until `docker compose ... down`. The data lives in the
`mymechanicshop-postgresql-data` volume and survives `down`. A host started with no
container fails with `Failed to connect to 127.0.0.1:5432`.

The local server is **passwordless** (`POSTGRES_HOST_AUTH_METHOD: trust`, port bound to
`127.0.0.1`) and the committed connection strings carry no `Password`. That is deliberate:
SonarQube's secrets analyzer reports any password in a connection string as a
vulnerability (`secrets:S6698`, `secrets:S6703`), dev value or not. Real environments get
their connection strings from outside the repo — environment variables
(`ConnectionStrings__Default`) or `appsettings.secrets.json`, which ABP's
`AddAppSettingsSecretsJson()` loads in both the host and the DbMigrator and `.gitignore`
excludes. The auth method is fixed by `initdb` when the volume is created, so changing it
means `docker compose -f etc/docker/infrastructure/postgresql.yml down -v`.

The connection strings say `Host=127.0.0.1`, **not `localhost`**. `localhost` resolves to
`::1` first, the port is published on IPv4 only, and on Windows a refused connection takes
~2 s before Npgsql falls back to IPv4 — on every new physical connection. A from-scratch
migration measured 36 s with `localhost` and 7 s with `127.0.0.1`.

Two Npgsql specifics, both in `MyMechanicShopEntityFrameworkCoreModule` and repeated in each
design-time factory:

- `AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true)` — ABP's documented
  setting. Npgsql 6+ otherwise maps `DateTime` to `timestamptz` and rejects non-UTC values.
- Unit-of-work transactions are **on**. The SQLite template had
  `AddAlwaysDisableUnitOfWorkTransaction()` because SQLite handles concurrent transactions
  poorly; PostgreSQL does not need it.

On a database's first creation the host log shows `[ERR] An error occurred using the
connection to database '…'` — EF probes the database, Postgres answers
`database "…" does not exist`, then EF creates it. Harmless and first-run only.

`Migrations/ABP/` is named for its contents: Identity, Tenants, Permissions, Settings,
AuditLogging, OpenIddict, BackgroundJobs, BlobStoring — the ABP framework's own tables.

Connection strings live in **two** `appsettings.json` files, and both need every entry:
`src/MyMechanicShop.DbMigrator` (used by the migrator *and*, via a relative base path, by
the host's design-time factory) and `src/MyMechanicShop.HttpApi.Host`. A module's
design-time factory does not read them, because a module can't know where the host lives.
It reads only the environment (`ConnectionStrings__<Name>`). `dotnet ef migrations add`
needs no database, so it works without one. Commands that query the database
(`migrations list`, `migrations remove` without `--force`) need the variable set.

Three things make a module's DbContext reach its own database, and **all three are
required**. Miss any one and you get a silently empty database — see the schema migrators section below.

1. The host DbContext must NOT own the module's entities. `MyMechanicShopDbContext` has
   no `[ReplaceDbContext(typeof(ICatalogDbContext))]`, does not implement
   `ICatalogDbContext`, and does not call `builder.ConfigureCatalog()`. With those
   present the module resolves to the host context on the `Default` connection string
   and its own connection string is inert.
2. **No `MigrationsAssembly` anywhere.** `MyMechanicShopEntityFrameworkCoreModule` calls
   a bare `options.UseNpgsql()`. EF's default looks for migrations next to the DbContext
   class, which is exactly where each module keeps them. A global
   `b.MigrationsAssembly(...)` pointing at `src` (the old design) would make every module
   context look in `src`. It would find nothing there, create an empty database and apply
   nothing, silently. The same happens if a module's design-time factory sets one.
3. The module's schema migrator is called by `MyMechanicShopDbMigrationService`.

### Module schema migrators: one interface per module

A module can't implement the host's `IMyMechanicShopDbSchemaMigrator`: it lives in
`src/MyMechanicShop.Domain`, which references the module, so that would be a cycle. Each
module mirrors the ABP template's own pattern instead:

| Project | Type |
| --- | --- |
| `<Name>.Domain` | `Data/I<Name>DbSchemaMigrator` + `Data/Null<Name>DbSchemaMigrator` (no-op fallback when no provider registers one) |
| `<Name>.EntityFrameworkCore` | `EntityFrameworkCore<Name>DbSchemaMigrator` — resolves the DbContext and calls `Database.MigrateAsync()` |

`MyMechanicShopDbMigrationService` injects `I<Name>DbSchemaMigrator` **explicitly** and calls
it after the host's migrators. That's one constructor parameter and one call per module.
It's chosen over a shared `IEnumerable<>` contract, which would have meant putting
infrastructure in the Shared Kernel.

The names matter. ABP exposes a class through `IFoo` **only when the class name ends with
`Foo`**. `EntityFrameworkCoreCatalogDbSchemaMigrator` and `NullCatalogDbSchemaMigrator` both
end with `CatalogDbSchemaMigrator`, so both are exposed as `ICatalogDbSchemaMigrator` with no
`[ExposeServices]`. The EF one wins because its module loads after the Domain module. A
migrator named any other way would leave the Null one in place.

**Forgetting the call in `MyMechanicShopDbMigrationService` is the silent failure**: the
migrator logs *"Successfully completed all database migrations"*
while leaving the module's database empty or missing. **After adding a module, check that
its database has tables** — none means no DDL ever executed:

```
docker exec mymechanicshop-postgresql psql -U postgres -d MyMechanicShopCatalog -c '\dt'
```

Under the old shared-interface design the same symptom came from a missing
`[ExposeServices]`, and it cost three debugging runs (back on SQLite, where it showed as a
0-byte file).

### Adding a module database

1. Connection string in both `appsettings.json` files.
2. In the module's `*.EntityFrameworkCore` project: package references to
   `Volo.Abp.EntityFrameworkCore.PostgreSql` and `Microsoft.EntityFrameworkCore.Design`
   (with the same `PrivateAssets` as Catalog's), and a `CatalogDbContextFactory`-style
   `IDesignTimeDbContextFactory`. It reads `<Name>DbProperties.ConnectionStringName` from the
   environment and sets **no** `MigrationsAssembly`.
3. The schema migrator trio from the table above, then one constructor parameter and one
   `MigrateAsync()` call in `MyMechanicShopDbMigrationService`.
4. Generate migrations **from the module's EF project**:

   ```
   cd modules/<Name>/src/MyMechanicShop.<Name>.EntityFrameworkCore
   dotnet ef migrations add Initial --output-dir Migrations
   ```

   The module has one DbContext, so `--context` isn't needed.
5. Its own in-memory database in the test harness (`project/testing.md`).
6. Verify from empty databases that its tables exist (the check above). The `add-module`
   skill walks through all of this, Angular included.

### Running the migrator

**In Development the host runs it.** `MyMechanicShopHttpApiHostModule.OnPreApplicationInitializationAsync`
calls `MyMechanicShopDbMigrationService.MigrateAsync()` — the DbMigrator's own service, so
migrations, schema migrators and seed contributors are unchanged; only the trigger moved.
It was added when `abp run` started every app at once and could not hold the host back
until a DbMigrator process exited. Under `just api` it remains a convenience: one command
gives a migrated host. The pre-initialization phase completes before any module's
`OnApplicationInitialization` touches the database and before the host listens.

The seed reads `OpenIddict:Applications` (the `_App` and `_Swagger` clients). That section
lives in the DbMigrator's `appsettings.json` **and** the host's `appsettings.Development.json`;
keep them in sync. Without it in the host, a fresh database gets no OpenIddict clients and
login fails.

Everywhere else — production, CI, or migrating without starting the host — run the
DbMigrator: `cd src/MyMechanicShop.DbMigrator && dotnet run`. **That `cd` is
load-bearing**: `Host.CreateDefaultBuilder` uses the working directory as the content
root, so from anywhere else the DbMigrator finds no `appsettings.json` and therefore no connection strings. The PostgreSQL container must be
up (`just infra`).

Databases are disposable: `docker compose -f etc/docker/infrastructure/postgresql.yml down -v`
deletes the volume, and the next `just api` (or DbMigrator run) recreates and reseeds both.
This is verified to work from scratch.

