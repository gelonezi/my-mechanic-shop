---
paths:
  - "**/test/**/*.cs"
  - "**/*Tests*/**/*.cs"
  - "**/*TestModule*.cs"
  - "**/*TestBase*.cs"
---

## Host test projects must not reference module test projects

`install-local-module` over-matches test packages. It wires every host test project —
plus `MyMechanicShop.HttpApi.Client.ConsoleTestApp`, which is an `Exe`, not a test project —
to all four of the module's `*.Tests` projects, and injects the matching
`[DependsOn(typeof(Catalog*TestModule))]` into their module classes. Both sides must be
removed after each install.

This is not just noise. `CatalogEntityFrameworkCoreTestModule` builds its own
`AbpUnitTestSqliteDatabase` containing only `CatalogDbContext`'s tables and overwrites
`AbpDbConnectionOptions.ConnectionStrings.Default`. Once the host's `TestBase` depends on
it, `MyMechanicShop.Domain.Tests` resolves to a database with none of the host's tables,
and `MyMechanicShopTestBaseModule.SeedTestData()` runs `IDataSeeder.SeedAsync()` against it.

Module tests are self-contained and run from the module's own solution. Keep it that way.


## Tests

`dotnet test MyMechanicShop.slnx` runs **4 tests**: 3 in
`MyMechanicShop.EntityFrameworkCore.Tests`, 1 in
`MyMechanicShop.Catalog.EntityFrameworkCore.Tests`.

`Domain.Tests` and `Application.Tests` report *"no tests available"* and that is
**correct, not a failure**. The ABP template writes its sample tests as abstract generic
classes (`SampleDomainTests<TStartupModule>`), which xUnit cannot instantiate. The EF Core
test project closes them:

```csharp
public class EfCoreSampleDomainTests : SampleDomainTests<MyMechanicShopEntityFrameworkCoreTestModule>;
```

So the test *logic* lives in the per-layer projects and is *executed* once per persistence
provider. Add MongoDB later and you re-run the same bodies by adding another set of closed
subclasses. Counting `[Fact]` per project therefore tells you nothing — look for the
concrete subclasses.

Beware: `dotnet test` prints "no tests available" as a warning and still exits 0. A project
that *should* run tests and silently stops looks identical to a passing one.

### One in-memory database per module

`MyMechanicShopEntityFrameworkCoreTestModule` builds a **separate**
`AbpUnitTestSqliteDatabase` per module and maps each to its own connection string
(`CatalogDbProperties.ConnectionStringName`, not a `"Catalog"` literal), mirroring the real
topology. Sharing one database would let a test join across a module boundary and pass,
certifying coupling that fails in production — worse once modules talk over events instead
of tables. No migrations are involved here; tables are created directly from each model.

These stay **SQLite in memory** although the application runs on PostgreSQL — ABP's own
templates do the same. The test modules override the EF module's `UseNpgsql` with
`UseSqlite`, so tests need no Docker; the trade-off is that provider-specific behaviour
(SQL translation, case sensitivity, timestamp types) is not exercised by them.

Adding a module: new `AbpUnitTestSqliteDatabase`, `CreateTables` for its DbContext, map its
`ConnectionStringName`, and dispose it in `OnApplicationShutdown`.

