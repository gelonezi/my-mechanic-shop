---
paths:
  - "**/*.csproj"
  - "**/*.slnx"
  - "**/*.abpmdl"
  - "**/*.abpsln"
  - "**/*Module.cs"
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
- **`new-package` projects import the repo root's `common.props`** (`..\..\..\..\common.props`,
  `AbpProjectType` = `app`). Copy a module's own `common.props` (Catalog's: `module`,
  `ConfigureAwait.Fody`) into the new module root, point the imports at `..\..\common.props`,
  and add a `FodyWeavers.xml` per project. Also: `TargetFrameworks` → `TargetFramework`.
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

