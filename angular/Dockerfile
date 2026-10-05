FROM node:24-alpine AS build

WORKDIR /app
# yarn.lock + --frozen-lockfile: install exactly the locked versions, fail if the lock is stale.
# --ignore-scripts: dependencies' install scripts never run inside the build.
COPY package.json yarn.lock /app/
RUN yarn install --frozen-lockfile --ignore-scripts
COPY . /app

RUN yarn build:prod

# Non-root nginx; it listens on 8080 (see nginx.conf).
FROM nginxinc/nginx-unprivileged:alpine
COPY dynamic-env.json /usr/share/nginx/html
COPY nginx.conf  /etc/nginx/conf.d/default.conf

COPY --from=build /app/dist/MyMechanicShop/browser /usr/share/nginx/html
