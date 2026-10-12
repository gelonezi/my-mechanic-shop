# MyMechanicShop task runner. `just` with no arguments lists the recipes.

set windows-shell := ["pwsh.exe", "-NoLogo", "-NoProfile", "-Command"]

compose := "docker compose -f etc/docker/infrastructure/postgresql.yml"

# List the recipes
default:
    @just --list

# Start the satellite applications in Docker (PostgreSQL on 127.0.0.1:5432) and wait until healthy
infra:
    {{ compose }} up -d --wait

# Run the API on https://localhost:44390; in Development it migrates and seeds every database first
[working-directory: 'src/MyMechanicShop.HttpApi.Host']
api: infra
    dotnet run
