---
paths:
  - "**/*.csproj"
  - "**/*.slnx"
  - "**/*Module.cs"
  - "**/Directory.Build.props"
  - "**/Directory.Build.targets"
  - "**/Directory.Packages.props"
  - "**/common.props"
  - "**/*.HttpApi.Host/package.json"
  - "**/abp.resourcemapping.js"
  - "**/wwwroot/**"
---

## Solutions: `.slnx` only

`MyMechanicShop.slnx` (Rider 2025.1+, `dotnet`, MSBuild) lists every project, host and modules.
Each module also has its own `modules/<Name>/<Name>.slnx`, which builds and tests just that
module. There is no `.sln` (`dotnet sln MyMechanicShop.slnx migrate` produces one if needed)
and **no ABP Studio solution**: the `.abpsln`, `.abpmdl` and `.abppkg` files were removed, so
these `.slnx` files are the only description of the solution. Nothing generates them: add a
module's projects to both by hand, in the root one under `/modules/<Name>/src/` and
`/modules/<Name>/test/` solution folders.

## Shared build settings and package versions

Every project — host and modules alike — picks these up from the repo root on its own,
with no `<Import>`:

| File | Holds |
| --- | --- |
| `Directory.Build.props` | defaults: `TargetFramework`, `Nullable`, `LangVersion`, `Version`, `NuGetAudit`, `AbpProjectType` = `app` |
| `Directory.Build.targets` | what must run after the SDK: the `CS1591`/`0436` `NoWarn` appends, and removals of ABP's `.abppkg` items (harmless now that the files are gone) |
| `Directory.Packages.props` | every package version (central package management) |

A module's own `modules/<Name>/common.props` adds only what is module-specific —
`AbpProjectType` = `module` and the `ConfigureAwait.Fody`/`Fody` references — and module
projects keep importing it (`..\..\common.props`). There is no root `common.props`.

So a `.csproj` carries neither versions nor the defaults above:

- `PackageReference` has **no `Version=`** — with one, restore fails with **NU1008**. A
  package missing from `Directory.Packages.props` fails with **NU1010**; add its
  `PackageVersion` there. ABP's templates write `Version=` (and the template's own ABP
  version, which can lag the solution's): strip the attributes and move any new package to
  the root file.
- No `TargetFramework`/`Nullable` — set them only to deviate from the root.

SonarCloud's Automatic Analysis does not evaluate `Directory.Build.props`, so it believes
nullable is off and flags every `null!` with **S8970**. Those are false positives: mark them
in SonarCloud, never delete the operator (the real build would raise CS8618).

## Local modules

`modules/MyMechanicShop.Catalog/` — business module: own database, migrations and Angular
library. `modules/MyMechanicShop.SharedKernel/` — value objects and enums shared by every
module, `Domain.Shared` + `Domain` only (see `project/shared-kernel.md`).

### Wiring a module into the host — by hand

Each host project references the module project of the **same layer** and depends on its
ABP module class. This is exactly Catalog's wiring:

| Host project (`src/`) | `ProjectReference` | `[DependsOn]` in the host's module class |
| --- | --- | --- |
| `MyMechanicShop.Domain.Shared` | `<Name>.Domain.Shared` | `<Name>DomainSharedModule` |
| `MyMechanicShop.Domain` | `<Name>.Domain` | `<Name>DomainModule` |
| `MyMechanicShop.Application.Contracts` | `<Name>.Application.Contracts` | `<Name>ApplicationContractsModule` |
| `MyMechanicShop.Application` | `<Name>.Application` | `<Name>ApplicationModule` |
| `MyMechanicShop.EntityFrameworkCore` | `<Name>.EntityFrameworkCore` | `<Name>EntityFrameworkCoreModule` |
| `MyMechanicShop.HttpApi` | `<Name>.HttpApi` | `<Name>HttpApiModule` |
| `MyMechanicShop.HttpApi.Client` | `<Name>.HttpApi.Client` | `<Name>HttpApiClientModule` |

- The module's `Installer` project is ABP tooling metadata; nothing references it.
- **Test projects stay out of it**: host tests never reference module test projects
  (`project/testing.md`).
- A module used by other modules (SharedKernel) is wired the same way between the modules:
  `Catalog.Domain.Shared → SharedKernel.Domain.Shared`, `Catalog.Domain → SharedKernel.Domain`.
  The host gets it transitively.

The full procedure for a new module — scaffold, strip ABP Studio metadata, wire, database,
tests, Angular library, recipes — is the `add-module` skill.

## The ABP CLI in this repo

| Command | Status |
| --- | --- |
| `abp new-module` | **Works** without the removed `.abpsln`: it writes a standalone module (with its own `.abpsln`, `.abpmdl`, `.abppkg`, `.cursor/`, `.abpstudio/`, `.editorconfig`, `.gitignore`) that has to be cleaned up and wired by hand — the `add-module` skill. Verified on ABP Studio CLI 3.1.4, whose templates reference ABP **10.6.1**. |
| `abp generate-proxy` | **Works**; run it through `just proxies`. |
| `abp install-local-module`, `abp add-package-ref`, `abp new-package` | **Unusable**: they read the `.abpmdl`/`.abpsln` files this repo no longer has. Do it by hand (table above). |
| `abp run` | Not used; the root `justfile` replaced it. |
| `abp install-libs` | **Needed**, through `just libs` (and `just api`, which runs it when `wwwroot/libs` is missing). See below. |

### The host's MVC client libraries: the one Yarn project

The API serves ABP's MVC login page (`Account.Web.OpenIddict` + LeptonX Lite), which bundles
jQuery, `abp.js`, Bootstrap, validation, … from `src/MyMechanicShop.HttpApi.Host/wwwroot/libs/`.
`abp install-libs` fills it: it runs `npx yarn` in the host folder (its own `package.json` +
`yarn.lock`, outside the pnpm workspace), then copies the files per `abp.resourcemapping.js`.
`wwwroot/libs/` and the host's `node_modules/` are gitignored, generated output.

- **`"packageManager": "yarn@1.22.22"` in the host's `package.json` is load-bearing.**
  Without it Yarn 1 reads the root `package.json`'s `pnpm@…` pin and refuses to install
  ("defines packageManager … However the current global version of Yarn is 1.22.22"). On a
  machine with a leftover `node_modules` the copy step still "works", which hides the
  failure until a fresh clone.
- If `wwwroot/libs` is missing the page still returns 200, but startup logs
  `Could not find the bundle file '/libs/…'` per file (31 of them) and the page runs without
  ABP's scripts. #15 deleted these files once; restored, verified with 0 bundle errors and all
  58 referenced assets returning 200.
- `wwwroot/global-styles.css`, `global-scripts.js` and `dev-login-helper.js` are referenced by
  `MyMechanicShopHttpApiHostModule.ConfigureBundles`; delete one only together with its
  `AddFiles`.

**Upgrading ABP** means three places, kept on one version: every `Volo.Abp.*` `PackageVersion`
in `Directory.Packages.props`, the `@abp/*` ranges in both Angular `package.json` files, then
`pnpm install` to refresh `pnpm-lock.yaml`. `abp update` was never tried against central
package management; editing the versions directly is the known path.

Rider's "add reference" quick-fix can insert a `<Reference HintPath="..\Users\<you>\.nuget\…">`
next to an existing `PackageReference` when a type is unresolved before restore. Remove it:
the path exists only on that machine and breaks CI.
