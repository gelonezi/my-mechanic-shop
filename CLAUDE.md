# MyMechanicShop

ABP Framework 10.7.0 on .NET 10, a study project shaped as a modular monolith: a layered DDD
host (`app` template) plus local ABP modules under `modules/`, each owning its PostgreSQL
database and migrations. Angular 22 UI, PostgreSQL 18 in Docker, multi-tenancy on.
ABP Studio's metadata (`.abpsln`, `.abpmdl`, `.abppkg`, run profiles) was removed: `.slnx`
files describe the solution, the root `justfile` runs it, and the `abp` CLI is used only to
scaffold, generate proxies and install the host's MVC client libraries.

Conventions live in `.claude/rules/`, loaded by their `paths:` when a matching file is
touched: `framework/` and `template/` hold ABP's general guidance (adapted where it
contradicted this repo), `project/` this repo's own. Multi-step procedures are skills in
`.claude/skills/`. This file keeps only what is true regardless of which file you open.

## Always prefer what ABP already provides

**Before writing a custom mechanism, check whether ABP ships one — and use it.** This is a
study project of ABP; a custom equivalent duplicates what the framework already localizes,
tests and documents. Verify in the installed package or the docs, and say which ABP
mechanism is being reused. Examples already in this repo:

- user input validation → DataAnnotations on DTOs (`[Required]`, `[StringLength(XxxConsts.MaxLength)]`):
  localized 400s from `AbpValidationResource`, already translated to pt-BR and 20 other languages
- domain invariants → `Check.NotNullOrWhiteSpace(value, nameof(value), maxLength: …)`, as ABP's own entities do
- business rules → `BusinessException` + namespaced error code + `MapCodeNamespace` (not for plain validation)
- enum display text → localization keys `Enum:<Type>.<numeric value>`; enums start with `Undefined = 0`
- value objects → ABP's `ValueObject` base class

## Where this repo's conventions live

| Topic | Rule (`.claude/rules/project/`) | Loads when touching |
| --- | --- | --- |
| `.slnx` only; `Directory.Build.*` + central package versions; wiring a module into the host by hand; what the `abp` CLI still does; the host's MVC client libraries | `solution-and-modules.md` | `*.csproj`, `*.slnx`, `*Module.cs`, `Directory.*`, `common.props`, the host's `package.json`, `abp.resourcemapping.js`, `wwwroot/**` |
| Shared Kernel: what may be shared; value object and enum conventions; mapping VOs in EF Core | `shared-kernel.md` | `modules/MyMechanicShop.SharedKernel/**`, `*Vo.cs`, `Enums/*.cs`, `*.EntityFrameworkCore/**/Configurations/*.cs` |
| One database per module; each module creates its own migrations, DbMigrator runs them all | `databases-and-migrations.md` | `*.EntityFrameworkCore/**/*.cs`, `*DbContext*.cs`, `Migrations/**`, `*DbMigrator*/**`, `*DbSchemaMigrator*.cs`, `appsettings*.json` |
| Auto API controller naming; Swagger tag vs URL; Angular proxy generation | `auto-api-controllers.md` | `*HttpApiModule*.cs`, `*AppService*.cs`, `*.Application.Contracts/**`, `proxy/**/*.ts` |
| Two Angular workspaces in one pnpm workspace; module UI ships as a package; root-relative `node_modules` paths | `angular-workspaces.md` | `angular/**/*.ts`/`.json`/`.ps1`, `public-api.ts`, `ng-package.json`, `pnpm-workspace.yaml` |
| Test discovery; one in-memory database per module; host tests never reference module tests | `testing.md` | `test/**/*.cs`, `*Tests*/**/*.cs`, `*TestModule*.cs`, `*TestBase*.cs` |
| `.editorconfig`, `.gitignore`, XML formatting of `.slnx` | `repo-config-files.md` | `.editorconfig`, `.gitignore`, `.vscode/settings.json` |

## Skills

| Skill | Use it to |
| --- | --- |
| `add-module` | add a business module end to end: `abp new-module`, strip ABP Studio metadata, wire into the host, own database + migrations, test database, Angular library, recipes |
| `add-entity` | add an entity with CRUD API, permissions, en + pt-BR localization, tests, migration, proxies and page, the way Catalog's `Product` is built |
| `ef-migration` | add, apply or remove a migration for the host or a module, and verify it reached the right database |
| `sonar-pr-issues` | list and triage SonarCloud findings on a PR, including this repo's false positives |

## Failures that report success

Worth knowing before the rule that explains them loads:

- a module's database stays empty while the migrator logs *"Successfully completed all
  database migrations"* — when `MyMechanicShopDbMigrationService` doesn't call that module's
  `I<Name>DbSchemaMigrator`, or a `MigrationsAssembly` points elsewhere. Check its tables.
- changing a module's app service or DTO doesn't regenerate its Angular proxies: stale ones
  build fine and break at runtime. Restart `just api`, then `just proxies`.
- registering a module in `angular/scripts/symlink-config.ps1` makes `ng build <lib>`
  die with exit 1 and **no output**
- `dotnet test` exits 0 for a project that discovers no tests

## Commands

Running goes through the root **`justfile`** ([just](https://just.systems)); Docker Desktop
must be running. `abp run` is not used. The run scripts are being migrated recipe by recipe
to `dotnet run` and pnpm.

```
just                # list the recipes
just infra          # satellite applications in Docker (PostgreSQL 18 on 127.0.0.1:5432), waits until healthy
just api            # infra + MVC client libs if missing, then `dotnet run` in src/MyMechanicShop.HttpApi.Host (https://localhost:44390); opens /swagger once healthy
just libs           # abp install-libs for the host (its MVC login page); rerun after an ABP upgrade
just web            # frozen pnpm install, builds Catalog once, then its watcher + ng serve --open (http://localhost:4200)
just web-install    # pnpm install --frozen-lockfile
just catalog-build  # builds the Catalog library into dist/catalog once
just proxies        # regenerates Catalog's Angular proxies; needs `just api` running
```

Run `just api` and `just web` in two terminals. On Windows the recipes run in `pwsh`
(`set windows-shell`), elsewhere in `sh`, so a recipe line must work in both. `api` uses
`[working-directory]` instead of a `cd`, and that directory is load-bearing: the content root
is the working directory, so from anywhere else the host (and the DbMigrator) finds no
`appsettings.json`. In Development the host migrates and seeds every database before it
listens. Outside Development, run `cd src/MyMechanicShop.DbMigrator && dotnet run` first.

```
dotnet build MyMechanicShop.slnx
dotnet test  MyMechanicShop.slnx          # 57 tests; see project/testing.md
```

Angular is **pnpm**: one workspace at the repo root (`pnpm-workspace.yaml`, one
`pnpm-lock.yaml`, pnpm pinned in the root `package.json`) covering both Angular folders.
Never `yarn` or `npm install` in them; other commands go through a filter, e.g.
`pnpm --filter ./angular exec ng lint`. The one Yarn project is the host's MVC client
libraries, driven only by `abp install-libs` (`just libs`).

## Notes

- `.claude/rules/` at the root is the single source for the whole repo, modules included:
  the `paths:` globs are `**/`-prefixed, so they reach into `modules/**` on their own.
  Adding a module needs no rules work. Do not create `.claude/` inside a module.
- `framework/` and `template/` started as conversions of the Cursor rules (`.mdc`) ABP
  generates. Those copies are no longer kept (`.cursor/` is gitignored, and `abp new-module`
  writes fresh ones into each new module, removed during `add-module`), so these rules are
  maintained here: when one contradicts this repo, fix it and point at the project rule or
  skill. `template/app.md` covers `src/` + `test/` and `template/module.md` covers
  `modules/**`, so app and module guidance don't bleed into each other.
- Keep CLAUDE.md small. It loads on every turn; a rule loads only when a matching file is
  touched, and a skill only when invoked. Anything tied to particular files belongs in a rule
  with a row in the index above; a repeatable procedure belongs in a skill; and anything that
  fails silently also gets a line in the list above, because a rule that never loads cannot
  warn you.
