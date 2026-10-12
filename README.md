# MyMechanicShop

[![CodeQL](https://github.com/gelonezi/my-mechanic-shop/actions/workflows/codeql.yml/badge.svg)](https://github.com/gelonezi/my-mechanic-shop/actions/workflows/codeql.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=gelonezi_my-mechanic-shop&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=gelonezi_my-mechanic-shop)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=gelonezi_my-mechanic-shop&metric=security_rating)](https://sonarcloud.io/component_measures?id=gelonezi_my-mechanic-shop&metric=security_rating)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=gelonezi_my-mechanic-shop&metric=reliability_rating)](https://sonarcloud.io/component_measures?id=gelonezi_my-mechanic-shop&metric=reliability_rating)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=gelonezi_my-mechanic-shop&metric=sqale_rating)](https://sonarcloud.io/component_measures?id=gelonezi_my-mechanic-shop&metric=sqale_rating)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=gelonezi_my-mechanic-shop&metric=vulnerabilities)](https://sonarcloud.io/project/issues?id=gelonezi_my-mechanic-shop&resolved=false&types=VULNERABILITY)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=gelonezi_my-mechanic-shop&metric=bugs)](https://sonarcloud.io/project/issues?id=gelonezi_my-mechanic-shop&resolved=false&types=BUG)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=gelonezi_my-mechanic-shop&metric=code_smells)](https://sonarcloud.io/project/issues?id=gelonezi_my-mechanic-shop&resolved=false&types=CODE_SMELL)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=gelonezi_my-mechanic-shop&metric=duplicated_lines_density)](https://sonarcloud.io/component_measures?id=gelonezi_my-mechanic-shop&metric=duplicated_lines_density)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=gelonezi_my-mechanic-shop&metric=ncloc)](https://sonarcloud.io/component_measures?id=gelonezi_my-mechanic-shop&metric=ncloc)

## Project for Practice in Realistic Scenarios and Goals

**Main Concepts:**
- **Core Architecture:** Modular Monolith.
- **Core Design**: [ABP Framework](https://abp.io) as a layered ***app template***.
- **Projects Design**: Domain-Driven Design with each business area as a local ABP module with its own database.

| High Level Slice    | Driven Concepts                                      |
| ------------------- | ---------------------------------------------------- |
| Backend             | C#, .NET, ABP Framework, EF Core                     |
| Frontend            | Angular,  ABP's Angular packages, LeptonX Lite theme |
| Database            | PostgreSQL, each module with it's own database       |
| Auth                | OpenIddict (ABP Account module), multi-tenancy       |
| Tooling             | just, pnpm, ABP Studio CLI (scaffolding, proxies)    |

## Localhost Prerequisites

* [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet)
* [Node.js](https://nodejs.org/en) `^24.15.0` (Angular 22's requirement)
* [pnpm](https://pnpm.io) 12: `corepack enable pnpm` (Corepack ships with Node 24; on Windows
  it needs an elevated terminal) or `winget install pnpm.pnpm`. The exact version is pinned
  in the root `package.json`, and pnpm switches to it on its own
* [Docker Desktop / Engine](https://www.docker.com/), **running**
* [just](https://just.systems) (`winget install Casey.Just`, `brew install just`, …); on
  Windows the recipes run in [PowerShell 7](https://learn.microsoft.com/powershell/) (`pwsh`)
* [ABP Studio CLI](https://abp.io), for scaffolding and proxy generation only

## Getting started

```bash
git clone https://github.com/gelonezi/my-mechanic-shop.git
cd my-mechanic-shop
just api    # terminal 1
just web    # terminal 2
```

| Recipe | What it does |
| --- | --- |
| `just` | lists the recipes |
| `just infra` | starts the satellite applications in Docker ([`etc/docker/infrastructure/`](etc/docker/infrastructure/postgresql.yml): PostgreSQL 18 on `127.0.0.1:5432`) and waits until they are healthy |
| `just api` | runs `infra`, then `dotnet run` in `src/MyMechanicShop.HttpApi.Host`, its content root |
| `just web` | installs the Angular workspaces (frozen lockfile), builds the Catalog library once, then runs its watcher and `ng serve` side by side |
| `just web-install` | `pnpm install --frozen-lockfile` |
| `just catalog-build` | builds the Catalog Angular library into `dist/catalog` once |
| `just proxies` | regenerates Catalog's Angular proxies from the running API (`just api` must be up) |

Open **http://localhost:4200** for the app. The API and Swagger UI are on **https://localhost:44390**. Log in as `admin`, with ABP's
default admin password (`MyMechanicShopConsts.AdminPasswordDefaultValue`). In Development
the host **migrates and seeds every database on startup**.

PostgreSQL keeps running after the host stops; `docker compose -f
etc/docker/infrastructure/postgresql.yml down` stops it, and the data survives in a Docker
volume.

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
* Add a migration. Each project owns its DbContext's migrations, and the DbMigrator applies
  them all:

  ```bash
  # host, from src/MyMechanicShop.EntityFrameworkCore
  dotnet ef migrations add <Name> --context MyMechanicShopDbContext --output-dir Migrations/ABP
  # Catalog module, from modules/MyMechanicShop.Catalog/src/MyMechanicShop.Catalog.EntityFrameworkCore
  dotnet ef migrations add <Name> --output-dir Migrations
  ```

* Outside Development the host does not migrate: run the DbMigrator before starting it,
  from its own folder (`cd src/MyMechanicShop.DbMigrator && dotnet run`).

## Solution structure

```
├── src/                          host application (layered: Domain.Shared → Domain →
│   │                             Application.Contracts → Application → HttpApi → Host)
│   ├── MyMechanicShop.HttpApi.Host     the API
│   ├── MyMechanicShop.DbMigrator       applies every database's migrations and seeds
│   └── MyMechanicShop.EntityFrameworkCore   the host's DbContext and its migrations
├── modules/
│   └── MyMechanicShop.Catalog/   local ABP module: its own layered projects (its EF
│       │                         project owns its migrations), tests and
│       └── angular/              Angular library (@my-mechanic-shop/catalog)
├── angular/                      the Angular application
├── test/                         host test projects
└── etc/
    └── docker/infrastructure/    PostgreSQL compose file
```

The Angular app consumes each module's UI as a built npm package (`dist/`), exactly as it
would from a registry, so the library must be rebuilt after a change. `just web` keeps it
rebuilt; `just catalog-build` builds it once. Both Angular folders are one pnpm workspace
with a single `node_modules` at the repo root.

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
