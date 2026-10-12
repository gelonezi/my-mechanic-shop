---
name: sonar-pr-issues
description: List and triage SonarCloud findings for a MyMechanicShop pull request (or main) through SonarCloud's public API, separating real issues from this repo's known false positives and from "fixes" that break ABP at runtime. Use when the user mentions Sonar or SonarCloud alerts, the quality gate, code smells or security hotspots on a PR.
---

# SonarCloud findings on a PR

The project is public (`gelonezi_my-mechanic-shop`), analysed by SonarCloud **Automatic
Analysis**: no scanner in CI, configuration in `.sonarcloud.properties` (exclusions only). The
check on a PR is "SonarCloud Code Analysis"; the quality gate can pass with open issues.

## Fetch

```
curl -s "https://sonarcloud.io/api/issues/search?componentKeys=gelonezi_my-mechanic-shop&pullRequest=<N>&resolved=false&ps=100"
curl -s "https://sonarcloud.io/api/hotspots/search?projectKey=gelonezi_my-mechanic-shop&pullRequest=<N>&ps=100"
```

Drop `pullRequest` for `main`. Per issue, report `rule`, `severity`, `type`, the file
(`component` after the `:`), `line` and `message`. A link that opens one rule's issues on the
PR: `https://sonarcloud.io/project/issues?id=gelonezi_my-mechanic-shop&pullRequest=<N>&rules=<rule>&resolved=false`.

## Triage

Read the flagged code before deciding; then sort each finding into fix / false positive /
won't fix, and say why.

Known false positives:

- **S8970** "Remove this null-forgiving operator; nullable warnings are disabled here".
  Nullable is enabled in the root `Directory.Build.props`, which Automatic Analysis does
  not evaluate. Removing `!` would make the real build emit CS8618. Mark as false positive.

Fixes that look right and break ABP — never apply them:

- **Sealing an application service** (any "make sealed" suggestion on a `*AppService`). ABP
  wraps app services in Castle proxies (unit of work, authorization, validation, auditing);
  a sealed class fails at resolution with `TypeLoadException … parent type is sealed`.
- **Dropping `?.` on a guarded parameter** (`value?.Trim()` → `value.Trim()` because the
  parameter is non-nullable). Callers can still pass `null`; without `?.` it throws
  `NullReferenceException` before ABP's `Check` raises the intended `ArgumentException`.
- **"Make this method static" (S2325) on a module's helper.** In a module, helpers are
  `protected virtual` so consumers can override them; that also satisfies S2325.

Usual real fixes:

- **S1186** empty method: a one-line comment saying why it's empty (ABP extension points
  such as `MyMechanicShopModuleExtensionConfigurator`), or remove the method.

## Hand-off

Marking issues False Positive / Won't Fix needs the user's SonarCloud account: give them the
rule-filtered link and a one-line justification per issue. Commit real fixes to the PR branch,
then re-run the fetch once the new analysis finishes to confirm the count dropped.
