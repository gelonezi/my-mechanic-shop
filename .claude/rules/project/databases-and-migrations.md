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
provider-agnostic — no SQLite package, no `Microsoft.EntityFrameworkCore.Design`, no
`Migrations/` folder — because modules must stay deployable against any provider.

| Database | Connection string | DbContext | Migrations |
| --- | --- | --- | --- |
| `MyMechanicShop.db` | `Default` | `MyMechanicShopDbContext` | `Migrations/ABP/` |
| `MyMechanicShopCatalog.db` | `Catalog` | `CatalogDbContext` | `Migrations/Catalog/` |

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
   options.UseSqlite(b =>
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
while leaving a 0-byte database. **After a migrator run, check the `.db` file sizes** — a
0-byte file means no DDL ever executed. This cost three debugging runs; every future module
migrator hits it identically.

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

`etc/scripts/migrate-database.ps1` does `Set-Location` into the project and then a bare
`dotnet run`. **That `Set-Location` is load-bearing**: the connection strings are relative
(`Data Source=../../MyMechanicShop.db;`), so they resolve against the *working directory*.
From the project folder `../../` is the repo root; from the repo root it is `C:\`. Rider's
run configuration and `dotnet run --project` both resolve correctly, but verify by where
the `.db` files land rather than assuming.

Databases are disposable: delete them, run the migrator, and both are recreated and
reseeded. This is verified to work from scratch.

