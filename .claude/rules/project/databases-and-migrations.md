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

Target architecture: **one database per module**, and `src/MyMechanicShop.DbMigrator`
owns *every* migration for *every* database. Point the DbMigrator at an environment and
its connection strings bring the whole environment up to date. Module EF projects stay
provider-agnostic — no provider package, no `Microsoft.EntityFrameworkCore.Design`, no
`Migrations/` folder — because modules must stay deployable against any provider.

The provider is **PostgreSQL**, one server holding one database per module:

| Database | Connection string | DbContext | Migrations |
| --- | --- | --- | --- |
| `MyMechanicShop` | `Default` | `MyMechanicShopDbContext` | `Migrations/ABP/` |
| `MyMechanicShopCatalog` | `Catalog` | `CatalogDbContext` | `Migrations/Catalog/` |

Locally the server is `etc/docker/infrastructure/postgresql.yml` (`postgres:18`, the
current major; pinned to the major so a minor update never needs `pg_upgrade`). It is
registered under the run profile's `containers.files`, and `abp run` starts it **before**
the applications (verified: the host process starts ~4.5 s after the container, which is
accepting connections ~0.2 s after start, ~2 s on a first `initdb`). Ctrl+C in `abp run`
stops and removes the container; the data lives in the `mymechanicshop-postgresql-data`
volume and survives. Killing `abp run` instead (closing its terminal) leaves the container
running; the next `abp run` reuses it. After Ctrl+C, wait for *All applications stopped*
before starting again: a new run that overlaps the old one's teardown shares the same compose
project, the teardown removes the container, and the new host fails with
`Failed to connect to 127.0.0.1:5432` (seen once; `docker compose ... up -d --wait` recovers it).

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
the design-time factories) and `src/MyMechanicShop.HttpApi.Host`.

Three things make a module's DbContext reach its own database, and **all three are
required**. Miss any one and you get a silently empty database — see the trap below.

1. The host DbContext must NOT own the module's entities. `MyMechanicShopDbContext` has
   no `[ReplaceDbContext(typeof(ICatalogDbContext))]`, does not implement
   `ICatalogDbContext`, and does not call `builder.ConfigureCatalog()`. With those
   present the module resolves to the host context on the `Default` connection string
   and its own connection string is inert.
2. `MyMechanicShopEntityFrameworkCoreModule` sets the migrations assembly globally:

   ```csharp
   options.UseNpgsql(b =>
       b.MigrationsAssembly(typeof(MyMechanicShopDbContext).Assembly.GetName().Name));
   ```

   EF otherwise looks for migrations next to the DbContext class — the module assembly,
   which has none. Setting this globally covers every present and future module; a
   per-context `options.Configure<TDbContext>(...)` form was tried and did not work.
3. A schema migrator per DbContext, carrying `[ExposeServices]`.

### The trap: `[ExposeServices]` on module schema migrators

`MyMechanicShopDbMigrationService` resolves `IEnumerable<IMyMechanicShopDbSchemaMigrator>`.
ABP's conventional registrar exposes a class through an interface `IFoo` **only when the
class name ends with `Foo`**:

- `EntityFrameworkCoreMyMechanicShopDbSchemaMigrator` ends with
  `MyMechanicShopDbSchemaMigrator` → exposed automatically.
- `EntityFrameworkCoreCatalogDbSchemaMigrator` ends with `CatalogDbSchemaMigrator` → **not**
  exposed. Without `[ExposeServices(typeof(IMyMechanicShopDbSchemaMigrator))]` it registers
  only as itself, never joins the collection, and its database is never migrated.

The failure is silent: the migrator logs *"Successfully completed all database migrations"*
while leaving the module's database empty or missing. **After adding a module, check that
its database has tables** — none means no DDL ever executed:

```
docker exec mymechanicshop-postgresql psql -U postgres -d MyMechanicShopCatalog -c '\dt'
```

This cost three debugging runs (back on SQLite, where it showed as a 0-byte file); every
future module migrator hits it identically.

### Adding a module database

1. Connection string in both `appsettings.json` files.
2. `CatalogDbContextFactory`-style `IDesignTimeDbContextFactory` in
   `src/MyMechanicShop.EntityFrameworkCore` reading that connection string.
3. Schema migrator in the same project, **with `[ExposeServices]`**.
4. Generate migrations into `src`, from `src/MyMechanicShop.EntityFrameworkCore`:

   ```
   dotnet ef migrations add Initial --context <Name>DbContext --output-dir Migrations/<Name>
   ```

5. Second in-memory database in the test harness (see the testing note below).

Nothing in step 2 of the previous list needs changing — the global `MigrationsAssembly`
already covers new contexts.

### Running the migrator

**In Development the host runs it.** `MyMechanicShopHttpApiHostModule.OnPreApplicationInitializationAsync`
calls `MyMechanicShopDbMigrationService.MigrateAsync()` — the DbMigrator's own service, so
migrations, schema migrators and seed contributors are unchanged; only the trigger moved.
Reason: `abp run` starts every application at once and its *Start and Wait For Ready*
action does not wait for a process to exit (tested), so a DbMigrator app in the run profile
races the host. The pre-initialization phase completes before any module's
`OnApplicationInitialization` touches the database and before the host listens.

The seed reads `OpenIddict:Applications` (the `_App` and `_Swagger` clients). That section
lives in the DbMigrator's `appsettings.json` **and** the host's `appsettings.Development.json`;
keep them in sync. Without it in the host, a fresh database gets no OpenIddict clients and
login fails.

Everywhere else — production, CI, or migrating without starting the host — run the
DbMigrator. `etc/scripts/migrate-database.ps1` does `Set-Location` into the project and then a bare
`dotnet run`. **That `Set-Location` is load-bearing**: `Host.CreateDefaultBuilder` uses the
working directory as the content root, so from anywhere else the DbMigrator finds no
`appsettings.json` and therefore no connection strings. The PostgreSQL container must be
up (`docker compose -f etc/docker/infrastructure/postgresql.yml up -d --wait`).

Databases are disposable: `docker compose -f etc/docker/infrastructure/postgresql.yml down -v`
deletes the volume, and the next host start (or DbMigrator run) recreates and reseeds both.
This is verified to work from scratch.

