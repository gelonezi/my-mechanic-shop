---
paths:
  - "**/*.csproj"
  - "**/*.slnx"
  - "**/*.abpmdl"
  - "**/*.abpsln"
  - "**/*Module.cs"
  - "**/Directory.Build.props"
  - "**/Directory.Build.targets"
  - "**/Directory.Packages.props"
  - "**/common.props"
---

## Two solution views, not kept in sync

| File | Consumed by | Contains |
| --- | --- | --- |
| `MyMechanicShop.abpsln` + `*.abpmdl` | ABP Studio, `abp` CLI | modules and their packages |
| `MyMechanicShop.slnx` | Rider, `dotnet`, MSBuild | every csproj file |

There is no `.sln` — the repo is `.slnx` only (Rider 2025.1+ reads it natively;
`dotnet sln MyMechanicShop.slnx migrate` produces a classic `.sln` if needed).

Nothing keeps these two in sync. Adding a module updates the `.abpmdl`/`.abpsln`
side only; the `.slnx` has to be edited by hand or Rider and `dotnet` never see the
module's projects.

## Shared build settings and package versions

Every project — host and modules alike — picks these up from the repo root on its own,
with no `<Import>`:

| File | Holds |
| --- | --- |
| `Directory.Build.props` | defaults: `TargetFramework`, `Nullable`, `LangVersion`, `Version`, `NuGetAudit`, `AbpProjectType` = `app` |
| `Directory.Build.targets` | what must run after the SDK: the `CS1591`/`0436` `NoWarn` appends, the `.abppkg` item removals |
| `Directory.Packages.props` | every package version (central package management) |

A module's own `modules/<Name>/common.props` adds only what is module-specific —
`AbpProjectType` = `module` and the `ConfigureAwait.Fody`/`Fody` references — and module
projects keep importing it (`..\..\common.props`). There is no root `common.props`.

So a `.csproj` carries neither versions nor the defaults above:

- `PackageReference` has **no `Version=`** — with one, restore fails with **NU1008**. A
  package missing from `Directory.Packages.props` fails with **NU1010**; add its
  `PackageVersion` there. ABP tooling (`new-module`, `new-package`, `add-package`,
  `install-local-module`) still writes `Version=`: after any of them, strip the attributes
  and move new versions to the root file.
- No `TargetFramework`/`Nullable` — set them only to deviate from the root.

## Local modules

`modules/MyMechanicShop.Catalog/` — business module. `modules/MyMechanicShop.SharedKernel/` —
value objects and enums shared by every module (see `project/shared-kernel.md`). Each has its
own `.slnx` and `.abpmdl`.

Install a local module into the host:

```
abp install-local-module "modules/<Name>/<Name>.abpmdl" -t "MyMechanicShop.abpmdl"
```

`-t` is mandatory when running from the solution root; without it the CLI errors with
`Please specify reference and target module`.

There is **no uninstall command** in ABP Studio CLI 3.1.1 (`uninstall-module`,
`remove-module` and variants are all unknown commands). Removal is either the ABP Studio
GUI (right-click the module under the target's imports → Uninstall Module) or manual:
the `imports` entry in the target `.abpmdl`, the `ProjectReference` entries, the
`[DependsOn]` attributes, and the projects in `.slnx`.

After installing, add the module's projects to `MyMechanicShop.slnx` under a
`/modules/<Name>/` solution folder. `install-local-module` does not do this.

Installing into another **module** (not the host) links projects by role —
`lib.domain-shared` → `lib.domain-shared`, `lib.domain` → `lib.domain` — with their
`[DependsOn]`. Check the result: it can also wire test projects (see `project/testing.md`).

## Creating a module or package with the CLI (ABP Studio CLI 3.1.1)

Verified the hard way while creating `MyMechanicShop.SharedKernel`:

- **A module with only the layers you need:** `abp new-module <Name> -t empty:empty
  -ts MyMechanicShop.abpsln -o modules/<Name>`, then one `abp new-package` per layer
  (`-t lib.domain-shared --add-localization`, `-t lib.domain`, …) with
  `-m modules/<Name>/<Name>.abpmdl -f src`. `empty:empty` is in the online docs but not in
  `abp help new-module`. The `ddd`/`standard` templates generate every layer.
- **`abp add-package-ref` resolves packages in the module closest to the current directory.**
  From the repo root that is the host's `MyMechanicShop.abpmdl`, so referencing a package of
  another module fails with `KeyNotFoundException: The given key '<Package>' was not present`.
  `cd modules/<Name>` first.
- **`new-package` projects import a repo-root `common.props`** (`..\..\..\..\common.props`)
  that no longer exists — the root `Directory.Build.*` files replaced it, so that import
  breaks the build. Copy an existing module's `common.props` (Catalog's) into the new module
  root, point the imports at `..\..\common.props`, and add a `FodyWeavers.xml` per project.
  Then trim the generated `.csproj` as in *Shared build settings* above: drop the
  `Version=` attributes and the `TargetFramework(s)`/`Nullable` properties.
- **There is no package builder for test projects.** `lib.test` is the role ABP writes into
  test projects' `.abppkg`, but `abp new-package -t lib.test` fails with *Package builder for
  lib.test is not defined!* — and still adds a `test` folder to the `.abpmdl`. Create the test
  project by hand: a `.csproj` like the module's others, an `.abppkg` with
  `{"role": "lib.test", "projectId": "<new guid>"}`, an entry under `packages` in the
  `.abpmdl`, and both `.slnx` files.
- The `empty:empty` template drops a `.gitignore` in the module root — delete it
  (`project/repo-config-files.md`). `new-package` adds projects to the **module's** `.slnx`
  only; the root `MyMechanicShop.slnx` is still manual.
- Rider's "add reference" quick-fix can insert a `<Reference HintPath="..\Users\<you>\.nuget\…">`
  next to an existing `PackageReference` when a type is unresolved before restore. Remove it:
  the path exists only on that machine and breaks CI.

