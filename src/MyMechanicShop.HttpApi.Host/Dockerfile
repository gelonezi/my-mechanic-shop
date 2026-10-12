# Build from the repository root: docker build -f src/MyMechanicShop.HttpApi.Host/Dockerfile .

# The login page's client libraries (wwwroot/libs, gitignored), restored by `abp install-libs` as `just libs` does
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS libs
RUN apk add --no-cache nodejs npm yarn \
    && dotnet tool install -g Volo.Abp.Studio.Cli --version 3.1.4
WORKDIR /src
COPY src/MyMechanicShop.HttpApi.Host/MyMechanicShop.HttpApi.Host.csproj src/MyMechanicShop.HttpApi.Host/package.json src/MyMechanicShop.HttpApi.Host/yarn.lock src/MyMechanicShop.HttpApi.Host/abp.resourcemapping.js ./
RUN /root/.dotnet/tools/abp install-libs

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY NuGet.Config Directory.Build.props Directory.Build.targets Directory.Packages.props ./
COPY modules modules
COPY src src
COPY --from=libs /src/wwwroot/libs src/MyMechanicShop.HttpApi.Host/wwwroot/libs
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish src/MyMechanicShop.HttpApi.Host -c Release -o /app \
    && mkdir /logs

# Chiseled: no shell or package manager, and non-root by default. "extra" keeps ICU, which pt-BR needs.
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra
WORKDIR /app
COPY --from=build /app .
# The binaries stay root-owned (read-only to the app); only Logs/, where Serilog writes, is the app's
COPY --from=build --chown=$APP_UID:$APP_UID /logs Logs
USER $APP_UID
# Plain HTTP on 8080 (the image default); TLS is the reverse proxy's job, or Kestrel's when a
# certificate is mounted (see etc/docker/docker-compose.yml)
EXPOSE 8080
# The health UI polls /health-status; relative, it would resolve against the bind address [::]
ENV App__HealthUiCheckUrl=http://localhost:8080/health-status
ENTRYPOINT ["dotnet", "MyMechanicShop.HttpApi.Host.dll"]
