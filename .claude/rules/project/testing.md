---
paths:
  - "**/test/**/*.cs"
  - "**/*Tests*/**/*.cs"
  - "**/*TestModule*.cs"
  - "**/*TestBase*.cs"
---

## Host test projects must not reference module test projects

Wire a module into the host's `src/` projects only. ABP's old `install-local-module` also
linked every host test project (and the `HttpApi.Client.ConsoleTestApp` `Exe`) to the
module's `*.Tests` projects with `[DependsOn(typeof(Catalog*TestModule))]`; that tool is
unusable now, but the mistake is easy to repeat by hand.

It is not just noise. `CatalogEntityFrameworkCoreTestModule` builds its own
`AbpUnitTestSqliteDatabase` containing only `CatalogDbContext`'s tables and overwrites
`AbpDbConnectionOptions.ConnectionStrings.Default`. Once the host's `TestBase` depends on
it, `MyMechanicShop.Domain.Tests` resolves to a database with none of the host's tables,
and `MyMechanicShopTestBaseModule.SeedTestData()` runs `IDataSeeder.SeedAsync()` against it.

Module tests are self-contained and run from the module's own solution. Keep it that way.


## Tests

`dotnet test MyMechanicShop.slnx` runs **57 tests**: 17 in
`MyMechanicShop.Catalog.EntityFrameworkCore.Tests`, 40 in
`MyMechanicShop.SharedKernel.Domain.Tests`. A module's own `.slnx` runs just its share. The
host's test projects hold only infrastructure (base classes, fixture, test module) since the
template's sample tests were deleted, so all of them report *"no tests available"*.

The kernel's tests are plain unit tests — concrete classes, no ABP application, no database —
because its VOs and validators need neither. Catalog's are integration tests and follow the
pattern below.

When host tests come back, follow the template's pattern. Even with tests, `Domain.Tests`
and `Application.Tests` report *"no tests available"*, and that is **correct, not a
failure**. The ABP template wrote its sample tests as abstract generic classes
(`SampleDomainTests<TStartupModule>`), which xUnit cannot instantiate. The EF Core test
project closes them:

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

