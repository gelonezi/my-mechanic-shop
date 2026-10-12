# Build from the repository root: docker build -f angular/Dockerfile .

FROM node:24-alpine AS build
ENV COREPACK_ENABLE_DOWNLOAD_PROMPT=0 NG_CLI_ANALYTICS=false
# pnpm at the version the root package.json pins
RUN corepack enable pnpm
WORKDIR /src
# One pnpm workspace at the root. Manifests first, so the install layer is reused until a dependency changes.
# Dependency install scripts stay off (allowBuilds in pnpm-workspace.yaml).
COPY package.json pnpm-lock.yaml pnpm-workspace.yaml ./
COPY angular/package.json angular/
COPY modules/MyMechanicShop.Catalog/angular/package.json modules/MyMechanicShop.Catalog/angular/
RUN pnpm install --frozen-lockfile
COPY angular angular
COPY modules/MyMechanicShop.Catalog/angular modules/MyMechanicShop.Catalog/angular
# The app consumes the Catalog library's build output (dist/catalog), so it is built first
RUN pnpm --filter ./modules/MyMechanicShop.Catalog/angular exec ng build catalog \
    && pnpm --filter ./angular exec ng build

# Non-root nginx; it listens on 8080 (see nginx.conf)
FROM nginxinc/nginx-unprivileged:1.30-alpine
COPY angular/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /src/angular/dist/MyMechanicShop/browser /usr/share/nginx/html
# Served at /getEnvConfig: mount a replacement to point the app at other URLs without rebuilding
COPY angular/dynamic-env.json /usr/share/nginx/html/
