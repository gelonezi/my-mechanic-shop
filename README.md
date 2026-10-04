# MyMechanicShop

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=gelonezi_my-mechanic-shop&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=gelonezi_my-mechanic-shop)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=gelonezi_my-mechanic-shop&metric=security_rating)](https://sonarcloud.io/component_measures?id=gelonezi_my-mechanic-shop&metric=security_rating)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=gelonezi_my-mechanic-shop&metric=reliability_rating)](https://sonarcloud.io/component_measures?id=gelonezi_my-mechanic-shop&metric=reliability_rating)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=gelonezi_my-mechanic-shop&metric=sqale_rating)](https://sonarcloud.io/component_measures?id=gelonezi_my-mechanic-shop&metric=sqale_rating)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=gelonezi_my-mechanic-shop&metric=vulnerabilities)](https://sonarcloud.io/project/issues?id=gelonezi_my-mechanic-shop&resolved=false&types=VULNERABILITY)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=gelonezi_my-mechanic-shop&metric=bugs)](https://sonarcloud.io/project/issues?id=gelonezi_my-mechanic-shop&resolved=false&types=BUG)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=gelonezi_my-mechanic-shop&metric=code_smells)](https://sonarcloud.io/project/issues?id=gelonezi_my-mechanic-shop&resolved=false&types=CODE_SMELL)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=gelonezi_my-mechanic-shop&metric=duplicated_lines_density)](https://sonarcloud.io/component_measures?id=gelonezi_my-mechanic-shop&metric=duplicated_lines_density)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=gelonezi_my-mechanic-shop&metric=ncloc)](https://sonarcloud.io/component_measures?id=gelonezi_my-mechanic-shop&metric=ncloc)
[![CodeQL](https://github.com/gelonezi/my-mechanic-shop/actions/workflows/codeql.yml/badge.svg)](https://github.com/gelonezi/my-mechanic-shop/actions/workflows/codeql.yml)

A study project: a **modular monolith** for a mechanic shop, built on the
[ABP Framework](https://abp.io) layered (`app`) template with Domain-Driven Design. Each
business area is a local ABP module with its own database; the first one, **Catalog**,
manages products.

| | |
| --- | --- |
| Backend | ABP Framework 10.6, .NET 10, EF Core |
| Frontend | Angular 22 with ABP's Angular packages, LeptonX Lite theme |
| Database | PostgreSQL 18 in Docker — one database per module |
| Auth | OpenIddict (ABP Account module), multi-tenancy enabled |
| Tooling | ABP Studio CLI 3.1 (`abp run`), yarn |

## Prerequisites

* [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet)
* [Node.js](https://nodejs.org/en) `^22.22.3` or `^24.15.0` (Angular 22's requirement); yarn is
  fetched through `npx`
* [Docker Desktop](https://www.docker.com/products/docker-desktop/), **running**
* ABP Studio CLI: `dotnet tool install -g Volo.Abp.Studio.Cli`

## Getting started

```bash
git clone https://github.com/gelonezi/my-mechanic-shop.git
cd my-mechanic-shop
./etc/scripts/initialize-solution.ps1   # once per clone
abp run
```

`initialize-solution.ps1` builds the solution, starts PostgreSQL, runs `abp install-libs`
(the host's `wwwroot/libs` is not committed), migrates and seeds the databases with the
DbMigrator, and creates the `openiddict.pfx` certificate.

Then open **http://localhost:4200** and log in as `admin`, with ABP's default admin password
(`MyMechanicShopConsts.AdminPasswordDefaultValue`). The API and Swagger UI are on
**https://localhost:44390**.

Stop with **Ctrl+C** and wait for *All applications stopped* before running `abp run` again.

## What `abp run` starts

`abp run` uses the `Default` run profile, [`etc/abp-studio/run-profiles/Default.abprun.json`](etc/abp-studio/run-profiles/Default.abprun.json).
It builds the .NET projects, starts the containers, then all applications:

| Entry | What it does |
| --- | --- |
| *Containers* | [`etc/docker/infrastructure/postgresql.yml`](etc/docker/infrastructure/postgresql.yml) — PostgreSQL 18 on `127.0.0.1:5432`; removed on Ctrl+C, data kept in a Docker volume |
| `MyMechanicShop.HttpApi.Host` | the API; in Development it **migrates and seeds the databases on startup** |
| `MyMechanicShop.Angular` | `ng serve` on http://localhost:4200 |
| `MyMechanicShop.Catalog.Angular` | `ng build catalog --watch`, rebuilding the Catalog UI library on every change |
| `MyMechanicShop.Catalog.Proxies` | waits for the host, regenerates Catalog's Angular proxies, then exits (shown as *Stopped*) |

`abp run --no-build` skips the .NET build. The runner hides each application's console
output: an entry stuck on *Starting* means its process failed — run its command by hand to
see why.

## Database

PostgreSQL 18 runs in Docker with two databases on one server:

| Database | Connection string | Owner |
| --- | --- | --- |
| `MyMechanicShop` | `Default` | ABP's framework modules (Identity, OpenIddict, tenants, …) |
| `MyMechanicShopCatalog` | `Catalog` | the Catalog module |

* The local server is **passwordless on purpose** (`trust` auth, reachable from this machine
  only), so no credential is committed. Real environments supply connection strings through
  environment variables (`ConnectionStrings__Default`) or an `appsettings.secrets.json`, which
  is gitignored.
* Reset everything: `docker compose -f etc/docker/infrastructure/postgresql.yml down -v`. The
  next host start recreates and reseeds both databases.
* Add a migration, from `src/MyMechanicShop.EntityFrameworkCore`:

  ```bash
  dotnet ef migrations add <Name> --context MyMechanicShopDbContext --output-dir Migrations/ABP
  dotnet ef migrations add <Name> --context CatalogDbContext --output-dir Migrations/Catalog
  ```

* Outside Development the host does not migrate: run `src/MyMechanicShop.DbMigrator`
  (`etc/scripts/migrate-database.ps1`) before starting it.

## Solution structure

```
├── src/                          host application (layered: Domain.Shared → Domain →
│   │                             Application.Contracts → Application → HttpApi → Host)
│   ├── MyMechanicShop.HttpApi.Host     the API, run by abp run
│   ├── MyMechanicShop.DbMigrator       migrates and seeds every database
│   └── MyMechanicShop.EntityFrameworkCore   DbContexts and the migrations of every module
├── modules/
│   └── MyMechanicShop.Catalog/   local ABP module: its own layered projects, tests and
│       └── angular/              Angular library (@my-mechanic-shop/catalog)
├── angular/                      the Angular application
├── test/                         host test projects
└── etc/
    ├── abp-studio/run-profiles/  what abp run starts
    ├── docker/infrastructure/    PostgreSQL compose file
    └── scripts/                  initialize-solution.ps1, migrate-database.ps1
```

The Angular app consumes each module's UI as a built npm package (`dist/`), exactly as it
would from a registry; `abp run` keeps it rebuilt.

## Tests

```bash
dotnet test MyMechanicShop.slnx
```

Tests use SQLite **in memory** — one database per module — so they need no Docker.

## Workflow and code quality

* `main` is protected: every change goes through a pull request, squash-merged, with titles
  following [Conventional Commits](https://www.conventionalcommits.org) (`feat:`, `fix:`,
  `refactor:`, `chore:`, `docs:`, …).
* Pull requests are checked by **CodeQL** (C# and TypeScript) and
  **[SonarQube Cloud](https://sonarcloud.io/project/overview?id=gelonezi_my-mechanic-shop)**
  automatic analysis; the badges at the top link to each metric. Generated code — EF
  migrations and Angular proxies — is excluded in [`.sonarcloud.properties`](.sonarcloud.properties).
  Automatic analysis does not collect test coverage, so there is no coverage badge.

  [![SonarQube Cloud](https://sonarcloud.io/images/project_badges/sonarcloud-dark.svg)](https://sonarcloud.io/summary/new_code?id=gelonezi_my-mechanic-shop)
* This repository's conventions, including the traps found along the way, are documented in
  [`CLAUDE.md`](CLAUDE.md) and [`.claude/rules/project/`](.claude/rules/project/).

## Deploying

Deploying follows ABP's [deployment documentation](https://abp.io/docs/latest/Deployment/Index).
Production needs its own signing certificate: ABP expects an `openiddict.pfx` file, which
can be created with

```bash
dotnet dev-certs https -v -ep openiddict.pfx -p <certificate password>
```

and whose password is read from `AuthServer:CertificatePassPhrase`. Two RSA certificates,
distinct from the HTTPS one (one for encryption, one for signing), are recommended — see
[OpenIddict certificate configuration](https://documentation.openiddict.com/configuration/encryption-and-signing-credentials.html#registering-a-certificate-recommended-for-production-ready-scenarios)
and ABP's [Configuring OpenIddict](https://abp.io/docs/latest/Deployment/Configuring-OpenIddict#production-environment).

## Resources

* [Angular application README](angular/README.md)
* [ABP layered application template](https://abp.io/docs/latest/solution-templates/layered-web-application)
* [ABP Web Application Development Tutorial](https://abp.io/docs/latest/tutorials/book-store/part-1)
