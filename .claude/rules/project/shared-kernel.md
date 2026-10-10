---
paths:
  - "**/MyMechanicShop.SharedKernel/**"
  - "**/*Vo.cs"
  - "**/Enums/*.cs"
  - "**/*.EntityFrameworkCore/**/Configurations/*.cs"
---

## Shared Kernel: what every module may share

`modules/MyMechanicShop.SharedKernel/` holds the small part of the model that all modules
deliberately share (DDD *Shared Kernel*). It exists because of dependency direction: the host
depends on the modules, never the reverse, so a VO in `src/MyMechanicShop.Domain` could not be
used by a module — `Catalog.Domain → MyMechanicShop.Domain → Catalog.Domain` is a cycle.

```
Catalog.Domain.Shared ─► SharedKernel.Domain.Shared ◄─ (next module).Domain.Shared
Catalog.Domain        ─► SharedKernel.Domain        ◄─ (next module).Domain
```

| Project | Holds |
| --- | --- |
| `SharedKernel.Domain.Shared` | enums, `*Consts` (max lengths), validators DTOs need (`EanValidator`), `CurrencyExtensions`, `SharedKernelResource` + `Localization/SharedKernel/{en,pt-BR}.json` |
| `SharedKernel.Domain` | value objects (`NameVo`, `DescriptionVo`, `MonetaryVo`, `EanVo`) |

No Application, HttpApi or EF layer. DTOs in a module's `Application.Contracts` see only
`Domain.Shared`, so they stay primitive (`string Name` with `[StringLength(NameConsts.MaxLength)]`)
and the application service builds the VO.

### What belongs here

Every change here recompiles every module, so the bar is high: **generic concepts with no
business rules, meaning the same thing in every context** — a name, a description, money, a
barcode, units. **Not** concepts that only look shared: Catalog's list price and an order line's
price (a snapshot with discounts and taxes) are different things; each module builds its own on
top of `MonetaryVo`.

### Value object conventions

- `sealed`, deriving from ABP's `Volo.Abp.Domain.Values.ValueObject`, `GetAtomicValues()` yielding
  the stored fields.
- **ABP's `ValueObject` does not override `Equals`/`GetHashCode`/`==`** — only `ValueEquals(object)`.
  Compare with `a.ValueEquals(b)`. A VO containing another VO must yield the inner one's
  *primitive* values, or the comparison falls back to reference equality.
- Immutable: `private set`, a private parameterless constructor for EF, a private constructor,
  and a static `Create(...)`. Single-value VOs expose `Value`.
- `Create` normalizes (trim, upper-case) and guards with ABP's `Check` (`NotNullOrWhiteSpace(…,
  maxLength)`, `Length`, `Range`) — an `ArgumentException`, i.e. a 500: a bug, not bad input.
  User input is rejected earlier by the DTO (DataAnnotations; a localized 400). When a rule can't
  be expressed as a DataAnnotation, it lives as a static validator in **`Domain.Shared`**
  (`EanValidator.IsValid`: GS1 mod-10 check digit) — DTOs see only `Domain.Shared`, so a method
  on the VO would be out of their reach. The DTO calls it from `IValidatableObject.Validate`
  (message from `SharedKernelResource`), and the VO's own `IsValid` delegates to it: one rule,
  two layers.
- Money is `decimal`, never `float`/`double`. `MonetaryVo` does not round: a unit price may need
  more places than the currency's `GetDecimals()`; round totals when presenting or charging.

### Enum conventions

- `Undefined = 0` first (ABP's own pattern), then **explicit values** — they are what EF stores,
  what the API sends, and the localization key. Never renumber or reuse one.
- Group related members in ranges of ten with gaps (`ProductUnit`: count 1–9, mass 10–19, …), or
  use a standard number when one exists (`Currency` = ISO 4217 numeric, `EanFormat` = digit count).
- Every member has a key `Enum:<Type>.<value>` in **both** `en.json` and `pt-BR.json`; a missing
  key shows the raw key in the UI. Angular: `'SharedKernel::Enum:ProductUnit.' + value | abpLocalization`.
- `ProductUnit` (what a product is sold by) and `ServiceUnit` (what a service is charged by) are
  separate on purpose: a product can never be sold by the hour.
- `Currency` is a closed list. To accept a new one: the ISO numeric code in the enum, a row in
  `CurrencyExtensions` (code, symbol, decimals — kept in code, not taken from ICU), and its names
  in both JSON files.

### Persisting value objects (EF Core)

Each module maps the kernel's VOs in its own EF configuration, as **complex types**
(`ComplexProperty`), not owned types (`OwnsOne`): no hidden key, value semantics — EF's
recommendation for VOs since EF 10. They are columns of the owner's table; name them after the
owner's property, or EF names them `Name_Value`:

```csharp
p.ComplexProperty(x => x.Name, n => n.Property(v => v.Value)
    .HasColumnName(nameof(Product.Name)).IsRequired().HasMaxLength(NameConsts.MaxLength));
p.ComplexProperty(x => x.Brand, n => n.Property(v => v.Value)        // NameVo? — optional
    .HasColumnName(nameof(Product.Brand)).HasMaxLength(NameConsts.MaxLength));
```

- **Optional VOs** (`NameVo? Brand`) need EF 10, and work because each kernel VO has at least one
  non-nullable property: an all-`NULL` set of columns reads back as a `null` VO. Verified by
  `ProductRepository_Tests` (round-trip, missing ones come back `null`).
- Multi-field VOs map each field: `MonetaryVo` → `PriceAmount` with `HasPrecision(18, 4)` and
  `PriceCurrency` (`StoreProductsConfigurations`).
- LINQ goes through the VO: `p.Name.Value`, `s.Price.Amount` — fine inside joins and projections.
- **Sorting by DTO field names breaks**: ABP's `CrudAppService` passes `Sorting` straight to
  Dynamic LINQ, and `name` is not a property path any more (`Name.Value` is). Override
  `ApplySorting` and translate through a field → path map (`ProductAppService.SortablePaths`).

### Using the kernel from a new module

```
abp install-local-module modules/MyMechanicShop.SharedKernel/MyMechanicShop.SharedKernel.abpmdl -t modules/<Name>/<Name>.abpmdl
```

Expect exactly two links, `<Name>.Domain.Shared → SharedKernel.Domain.Shared` and
`<Name>.Domain → SharedKernel.Domain`, each with its `[DependsOn]`. The host needs no install
of its own: it gets the kernel transitively through the modules.
