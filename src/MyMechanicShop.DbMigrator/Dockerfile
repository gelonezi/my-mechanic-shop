# Build from the repository root: docker build -f src/MyMechanicShop.DbMigrator/Dockerfile .

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY NuGet.Config Directory.Build.props Directory.Build.targets Directory.Packages.props ./
COPY modules modules
COPY src src
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish src/MyMechanicShop.DbMigrator -c Release -o /app \
    && mkdir /logs

# Chiseled: no shell or package manager, and non-root by default. "extra" keeps ICU, which pt-BR needs.
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra
WORKDIR /app
COPY --from=build /app .
# The binaries stay root-owned (read-only to the app); only Logs/, where Serilog writes, is the app's
COPY --from=build --chown=$APP_UID:$APP_UID /logs Logs
USER $APP_UID
ENTRYPOINT ["dotnet", "MyMechanicShop.DbMigrator.dll"]
