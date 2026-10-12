# MyMechanicShop

ABP Framework 10.7.0 on .NET 10. Layered DDD (`app` template), Angular UI, PostgreSQL
(Docker), multi-tenancy on. Scaffolded with ABP Studio CLI 3.1.1 — on SQLite, since replaced.

General ABP conventions live in `.claude/rules/framework/` and `.claude/rules/template/`,
scoped by `paths:`. This repo's own conventions live in `.claude/rules/project/` under the
same mechanism — this file keeps only what is true regardless of which file you open.

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

| Topic | Rule | Loads when touching |
| --- | --- | --- |
| `.slnx` vs `.abpsln`; `Directory.Build.*` + central package versions; installing local modules; creating modules/packages with the CLI | `project/solution-and-modules.md` | `*.csproj`, `*.slnx`, `*.abpmdl`, `*Module.cs`, `Directory.*.props`/`.targets`, `common.props` |
| Shared Kernel: what may be shared; value object and enum conventions; mapping VOs in EF Core | `project/shared-kernel.md` | `modules/MyMechanicShop.SharedKernel/**`, `*Vo.cs`, `Enums/*.cs`, `*.EntityFrameworkCore/**/Configurations/*.cs` |
| One database per module; each module creates its own migrations, DbMigrator runs them all | `project/databases-and-migrations.md` | `*.EntityFrameworkCore/**`, `*DbContext*.cs`, `Migrations/**`, `appsettings*.json` |
| Auto API controller naming; Swagger tag vs URL; proxy generation | `project/auto-api-controllers.md` | `*HttpApiModule*.cs`, `*AppService*.cs` |
| Two Angular workspaces; module UI ships as an npm package | `project/angular-workspaces.md` | `angular/**`, `public-api.ts` |
| Test discovery; one in-memory database per module | `project/testing.md` | `test/**`, `*Tests*/**`, `*TestModule*.cs` |
| `.editorconfig`, `.gitignore` | `project/repo-config-files.md` | `.editorconfig`, `.gitignore` |

Three failures documented in those rules are **silent** — they report success or nothing at
all. Worth knowing they exist before the rule that explains them loads:

- a module's database can be created empty while the migrator logs
  *"Successfully completed all database migrations"* (check that its tables exist)
- registering a module in `angular/scripts/symlink-config.ps1` makes `ng build <lib>`
  die with exit 1 and **no output**
- `dotnet test` exits 0 for a project that discovers no tests

## Commands

Running is done through the root **`justfile`** ([just](https://just.systems)); Docker
Desktop must be running. `abp run` and ABP Studio run profiles are **not** used. The `abp`
CLI is still used for scaffolding and proxy generation. The run scripts are being migrated
recipe by recipe to `dotnet run` and pnpm; how the frontend runs is not decided yet.

```
just          # list the recipes
just infra    # satellite applications in Docker (PostgreSQL 18 on 127.0.0.1:5432), waits until healthy
just api      # runs `infra` first, then `dotnet run` in src/MyMechanicShop.HttpApi.Host (https://localhost:44390)
```

On Windows the recipes run in `pwsh` (`set windows-shell`), elsewhere in `sh`, so a recipe
line must work in both. `api` uses `[working-directory]` instead of a `cd`, and that
directory is load-bearing. The content root is the working directory, so from anywhere else
the host (and the DbMigrator) finds no `appsettings.json`. In Development the host
migrates and seeds every database before it listens: `MyMechanicShopHttpApiHostModule` runs
the DbMigrator's own migration service, ~3 s per start when there is nothing to migrate.
Outside Development, run `cd src/MyMechanicShop.DbMigrator && dotnet run` first.

The Catalog Angular library and its proxies are no longer rebuilt or regenerated for you;
see `project/angular-workspaces.md` and `project/auto-api-controllers.md`.

```
dotnet build MyMechanicShop.slnx
dotnet test  MyMechanicShop.slnx          # 57 tests; see project/testing.md
```

Angular is **yarn, not npm**: both workspaces ship a `yarn.lock` and no
`package-lock.json`, and `angular/start.ps1` is `npx yarn` then `npx yarn start`.
In `angular/`: `yarn start` (`ng serve`), `yarn test`, `yarn lint`.

## Notes

- `.cursor/rules/` is generated by ABP — ABP's default AI target is Cursor — and is
  **gitignored** via an unanchored `.cursor/` pattern, at the root and inside every module
  (`modules/<Name>/.cursor/rules/`, which ABP recreates on every scaffold; 12 of the 13 in
  the Catalog module are byte-identical to the root copy). Ignored rather than deleted,
  because deleting is a fight you lose on the next `abp new-module`, and the files stay
  usable for anyone working in Cursor. `.claude/rules/` is the maintained copy.
- `.claude/rules/` at the root is the single source for the whole repo, modules included:
  the `paths:` globs are `**/`-prefixed, so they reach into `modules/**` on their own.
  Adding a module needs no rules work. Three subtrees, by origin:
  `framework/` and `template/` are converted from ABP's generated `.mdc` files and left
  as ABP wrote them; `project/` is this repo's own, and is where new conventions go.
  Two scopes are deliberately narrow — `template/app.md` covers `src/`+`test/` and
  `template/module.md` covers `modules/**`, so app- and module-template guidance don't
  bleed into each other.
- Do not create `.claude/` inside a module. If ABP ever ships a genuinely new `.mdc`
  (as `template/module.mdc` was), convert that one file into the root `.claude/rules/`.
  **After an ABP upgrade, check for new `.mdc` files deliberately** — now that `.cursor/`
  is gitignored, a new one arrives already ignored and `git status` will never mention it:

  ```
  find . -path '*/.cursor/*' -name '*.mdc' -not -path '*/node_modules/*' -printf '%f\n' | sed 's/\.mdc$//' | sort -u
  ```

  Search every `.cursor` directory, not just the root one — `module.mdc` ships only inside
  `modules/<Name>/.cursor/rules/template/`, so a root-only search misses it.

  Expect **15** names, all already covered: 14 map to a same-named
  `.claude/rules/**/*.md`, and `mcp-studio` became `.claude/skills/abp-studio-mcp/`
  instead — a skill, because it describes tools to invoke rather than conventions scoped
  to files. Any 16th name is new and needs converting. `.claude/rules/project/` has no
  `.mdc` counterpart; it is ours.
- Keep CLAUDE.md small. It loads on every turn; a `project/` rule loads only when a
  matching file is touched. Anything tied to particular files belongs in a rule, with a
  row in the index above — and, if it fails silently, a line in the warning list, because
  a rule that never loads cannot warn you.
- `.vscode/mcp.json` is gitignored by the template's `.gitignore`; `.mcp.json` is not.
