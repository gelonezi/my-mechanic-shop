import { Environment } from '@abp/ng.core';

/**
 * Used by `abp generate-proxy -t ng` as the --source project: the CLI reads the
 * API definition URL and rootNamespace from here.
 *
 * Keep the urls as plain string literals. The schematic parses this file
 * statically, so a `const` indirection (url: someVar) makes it fail with
 * "Cannot resolve API URL for ... remote service name".
 *
 * `Catalog` matches the apiName the generated services declare and the entry of
 * the same name in the HOST app's environment.ts. Today both point at the
 * modular monolith on 44390; when Catalog becomes its own pod, these two urls
 * are the only values that change.
 */
export const environment = {
  production: false,
  application: {
    name: 'Catalog',
  },
  apis: {
    default: {
      url: 'https://localhost:44390',
      rootNamespace: 'MyMechanicShop.Catalog',
    },
    Catalog: {
      url: 'https://localhost:44390',
      rootNamespace: 'MyMechanicShop.Catalog',
    },
  },
} as Environment;
