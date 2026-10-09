---
paths:
  - "**/.editorconfig"
  - "**/.gitignore"
---

# Repo config files: one of each, at the root

- One `.editorconfig`, at the root, with `root = true`. It covers the whole repo including
  `modules/**` and both Angular workspaces. Both ABP and the Angular CLI scaffold their own
  copies — `modules/<Name>/.editorconfig` and `modules/<Name>/angular/.editorconfig` — so
  delete both after creating a module. The Angular one matters most: it carries its own
  `root = true`, which cuts that whole tree off from the root file.
- The Angular settings in `.editorconfig` are scoped to `angular/**` and `modules/*/angular/**`
  rather than `[*]` on purpose. Angular's stock file puts `indent_size = 2` and
  `trim_trailing_whitespace = true` under `[*]`; hoisting that to the root would reindent the
  C# tree and strip trailing whitespace from 9 ABP template files.
- One `.gitignore`, at the root, covering the whole repo. ABP and the Angular CLI scaffold
  their own into every new module (`modules/<Name>/.gitignore` and
  `modules/<Name>/angular/.gitignore`) — delete both. Unlike `.editorconfig` there is no
  `root` switch: root patterns already apply at every depth and a nested file only ADDS
  rules, so nothing here needs per-module work. `.idea/**/.gitignore` is Rider's own,
  lives inside the ignored `.idea/`, and is regenerated — leave it alone.
- The Angular patterns in `.gitignore` are deliberately unanchored (`dist/`, `.angular/`,
  `coverage/`) rather than the Angular CLI's anchored form (`/dist`, `/.angular/cache`).
  A separator at the **start or middle** of a pattern anchors it to the `.gitignore`'s own
  directory, so `/dist` or `.angular/cache/` at the root would only ever match
  `<repo-root>/dist` — never `angular/dist` or `modules/<Name>/angular/`. Do not "tidy"
  these back into the CLI's form. Verify any change with:
  `git ls-files --others --exclude-standard | sort` before and after — the diff must show
  no newly visible files.
- `.vscode` follows the same rule but needs `**/.vscode/*`, not `**/.vscode/`. Ignoring the
  **directory** stops git descending into it, and a file under an excluded directory can
  never be re-included — the `!` whitelist below it would silently stop working. Excluding
  the *contents* keeps the negations alive. The whitelist is root-anchored on purpose
  (`!/.vscode/settings.json`, leading `/`): shared editor config is committed once at the
  root, while `angular/.vscode/` and `modules/<Name>/angular/.vscode/` are CLI scaffolding
  — duplicated `extensions.json` files — and are ignored wholesale.
- The root `.gitignore`'s .NET half comes from the module template's copy, which tracked a
  newer github/gitignore VisualStudio template than the solution root's did. That is where
  `[Ll]ogs/`, `FodyWeavers.xsd`, `*.tlog`, `.vshistory/` and `MigrationBackup/` come from.
- `appsettings.secrets.json` is ignored everywhere. It is what ABP's
  `AddAppSettingsSecretsJson()` loads, so it is where real connection strings and
  OpenIddict client secrets belong — never commit one.
- Because it is never committed, every `.csproj` that copies it to the output (DbMigrator,
  TestBase, ConsoleTestApp) does so with `Condition="Exists('appsettings.secrets.json')"`.
  Without the condition a fresh clone fails to build with `MSB3030` — silently fine on any
  machine that has the file. All three load it with `optional: true` (ABP's
  `AddAppSettingsSecretsJson` defaults to it), so a missing file is fine at runtime. A new
  project that copies it needs the same condition.
