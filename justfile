# MyMechanicShop task runner. `just` with no arguments lists the recipes.

set windows-shell := ["pwsh.exe", "-NoLogo", "-NoProfile", "-Command"]

compose := "docker compose -f etc/docker/infrastructure/postgresql.yml"
host := "src/MyMechanicShop.HttpApi.Host"
app := "./angular"
catalog := "./modules/MyMechanicShop.Catalog/angular"

# List the recipes
default:
    @just --list --unsorted

# Start the satellite applications in Docker (PostgreSQL on 127.0.0.1:5432) and wait until healthy
infra:
    {{ compose }} up -d --wait

# Run the API on https://localhost:44390; in Development it migrates and seeds every database first
[working-directory: 'src/MyMechanicShop.HttpApi.Host']
api: infra libs-if-missing
    dotnet run

# Restore the API's MVC client libraries (the login page) into wwwroot/libs; rerun after an ABP upgrade
libs:
    abp install-libs -wd {{ host }}

[private]
@libs-if-missing:
    {{ if path_exists(justfile_directory() / host / "wwwroot/libs/abp") == "true" { "echo 'MVC client libraries present (just libs refreshes them)'" } else { "just libs" } }}

# Run the Angular app on http://localhost:4200, rebuilding the Catalog library on every change
web: web-install catalog-build web-dev

# Install the Angular workspaces exactly as locked (one pnpm workspace at the repo root)
web-install:
    pnpm install --frozen-lockfile

# Build the Catalog Angular library into dist/catalog, which the app consumes
catalog-build:
    pnpm --filter {{ catalog }} exec ng build catalog

# Regenerate the Catalog Angular proxies from the running API (start it with `just api`)
[working-directory: 'modules/MyMechanicShop.Catalog/angular']
proxies: && proxies-lf
    curl -sSkf --max-time 5 https://localhost:44390/health-status
    abp generate-proxy -t ng -m catalog -s dev-app --target catalog -a Catalog

# The generator writes some files CRLF. .gitattributes pins proxy/ to LF in the repository;
# rewriting them on disk too keeps `git status` clean when the API did not change.
[private]
[script('node')]
[working-directory: 'modules/MyMechanicShop.Catalog/angular/projects/catalog/src/lib/proxy']
proxies-lf:
    const fs = require('node:fs');
    for (const file of fs.readdirSync('.', { recursive: true })) {
      if (!fs.statSync(file).isFile()) continue;
      const text = fs.readFileSync(file, 'utf8');
      if (text.includes('\r\n')) fs.writeFileSync(file, text.replace(/\r\n/g, '\n'));
    }

[parallel]
[private]
web-dev: catalog-watch web-serve

[private]
catalog-watch:
    pnpm --filter {{ catalog }} exec ng build catalog --watch

[private]
web-serve:
    pnpm --filter {{ app }} exec ng serve
