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
| `MyMechanicShop.slnx` | Rider, `dotnet`, MSBuild | the 26 csproj files |

There is no `.sln` — the repo is `.slnx` only (Rider 2025.1+ reads it natively;
`dotnet sln MyMechanicShop.slnx migrate` produces a classic `.sln` if needed).

Nothing keeps these two in sync. Adding a module updates the `.abpmdl`/`.abpsln`
side only; the `.slnx` has to be edited by hand or Rider and `dotnet` never see the
module's projects.

## Local modules

`modules/MyMechanicShop.Catalog/` — local ABP module with its own `.slnx` and `.abpmdl`.

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

