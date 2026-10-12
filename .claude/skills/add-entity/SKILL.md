---
name: add-entity
description: Add a new entity (aggregate root) with its CRUD API, permissions, localization, tests, migration, Angular proxies and page to a MyMechanicShop module, built the way Catalog's Product is. Use when the user asks to add an entity, aggregate, CRUD screen or API resource to a module, or to extend one with new fields.
---

# Add an entity to a module

Model every step on Catalog's `Product`; the files named below are the reference. `<Entity>` /
`<Entities>` are the singular / plural PascalCase names, `<Module>` the module (e.g.
`Catalog`). For a field added to an existing entity, run the same steps and touch only what
the field needs.

Before writing a mechanism, check whether ABP ships one (CLAUDE.md): validation, guards,
error codes, enum text and value objects all have an ABP way here.

## Domain

- `Domain/<Entities>/<Entity>.cs` — like `Products/Product.cs`: `FullAuditedAggregateRoot<Guid>`,
  private setters, a private parameterless constructor for EF, a public constructor taking
  `id` (never generate GUIDs inside), fluent `Set…` methods returning `this`, invariants with
  ABP's `Check` (an `ArgumentException`: a bug, not bad input).
- Shared concepts are Shared Kernel value objects (`NameVo`, `DescriptionVo`, `MonetaryVo`,
  `EanVo`) and enums (`project/shared-kernel.md`). New enums: `Undefined = 0`, explicit
  values, never renumbered.
- Business rules break with `BusinessException("<Module>:<Code>")`; the code is a constant in
  `<Module>ErrorCodes` and has a localized message (`MapCodeNamespace` is already configured).
- A repository interface only if you need custom queries; otherwise `IRepository<<Entity>, Guid>`.

## EF Core

- `EntityFrameworkCore/Configurations/<Entities>Configurations.cs`, an `internal static`
  extension like `ProductsConfigurations`: `ToTable(<Module>DbProperties.DbTablePrefix + "<Entities>", <Module>DbProperties.DbSchema)`,
  `ConfigureByConvention()`, value objects as **`ComplexProperty`** with explicit column
  names (`project/shared-kernel.md`), enums as their stored numbers.
- Call it from `Configure<Module>()` in `<Module>DbContextModelCreatingExtensions`; add the
  `DbSet` to `<Module>DbContext`.
- Seed data, if any: an `IDataSeedContributor` like `ProductsDataSeederContributor` (host
  side only: `context.TenantId is null`; idempotent).
- Migration: the `ef-migration` skill, from the module's EF project.

## Contracts and application

- `Application.Contracts/<Entities>/`: `<Entity>Dto`, `CreateUpdate<Entity>Dto`,
  `I<Entity>AppService`. DTOs stay primitive (strings, not VOs) and validate with
  DataAnnotations bound to the kernel's constants: `[Required]`,
  `[StringLength(NameConsts.MaxLength)]`. Rules DataAnnotations can't express go in
  `IValidatableObject.Validate`, calling the shared validator (e.g. `EanValidator`).
- Permissions in `<Module>Permissions` (`Default` / `Create` / `Edit` / `Delete`) and
  `<Module>PermissionDefinitionProvider`, with the right `MultiTenancySides` (Catalog's
  products are `Host`).
- `Application/<Entities>/<Entity>AppService.cs` like `ProductAppService`: a
  `CrudAppService<…>` **not sealed**, overridable members `protected virtual` (ABP's Castle
  proxies subclass it; a module must be overridable), policy names set in the constructor,
  `LocalizationResource` and `ObjectMapperContext` set. Build VOs from the DTO in
  `MapToEntityAsync`.
- Sorting: if the entity has VOs, override `ApplySorting` with a field → property-path map
  like `ProductAppService.SortablePaths`, or Dynamic LINQ fails on `Name` (it is `Name.Value`).
- Mapping: a Mapperly `MapperBase<<Entity>, <Entity>Dto>` like
  `Products/Mappers/ProductToProductDtoMapper.cs`, with private `Map…` methods turning VOs
  into primitives.

## API surface

Add the entity's arm to `UrlControllerNameNormalizer` in `<Module>HttpApiModule`:
`"<Entity>" => "<Entities>"`, **PascalCase, never kebab-case** — the value also names the
generated Angular service (`project/auto-api-controllers.md`). Result: tag `<Entity>`, route
`/api/<module>/<entities-kebab>`.

## Localization

Every new key in **both** `Localization/<Module>/en.json` and `pt-BR.json`:
`Menu:<Entities>`, `Permission:<Entities>` (+ `.Create`/`.Edit`/`.Delete`), field labels, each
error code, and `Enum:<Type>.<value>` per member of a new enum (a missing key shows the raw
key in the UI). Don't write tests that assert localization keys or values; test behaviour.

## Tests

In the module's test projects, following `Products/`:

- abstract generic `…_Tests<TStartupModule>` in `Domain.Tests` (repository/domain behaviour)
  and `Application.Tests` (app service: create/get/update/list/sort, validation failures),
- closed once in `EntityFrameworkCore.Tests` (`EfCore<Entity>AppService_Tests`,
  `EfCore<Entity>Repository_Tests`).

Run `dotnet test modules/MyMechanicShop.<Module>/MyMechanicShop.<Module>.slnx`. A project that
should run tests and reports "no tests available" still exits 0: check the count.

## Angular

1. Restart `just api` (the generator reads the running API's definition), then `just proxies`.
   Forgetting this is silent: stale proxies build and fail at runtime.
2. Page component in the module's library (`projects/<module>/src/lib/<entity>/`, like
   `lib/product/`), exported from `public-api.ts`.
3. Route in `lib/<module>.routes.ts` with `data: { requiredPolicy: '<Module>.<Entities>' }`.
4. Menu entry in `config/src/providers/route.provider.ts`, `parentName` the module's entry,
   `requiredPolicy` on the **child** (see the comment there on tenant-side permissions), and
   a name in `config/src/enums/route-names.ts`.
5. With `just web` running, the library watcher rebuilds and `ng serve` reloads.

## Done when

`dotnet build MyMechanicShop.slnx` and both test suites pass, the migration is applied
(tables checked, `ef-migration` skill), `git status` shows the regenerated proxies, and the
page works at http://localhost:4200 for a user with the permission.
