import type { CreateUpdateProductDto, ProductDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedAndSortedResultRequestDto, PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ProductsService {
  private restService = inject(RestService);
  apiName = 'Catalog';

  create = (input: CreateUpdateProductDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductDto>(
      {
        method: 'POST',
        headers: { Accept: 'application/json' },
        url: '/api/catalog/products',
        body: input,
      },
      { apiName: this.apiName, ...config },
    );

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>(
      {
        method: 'DELETE',
        url: `/api/catalog/products/${id}`,
      },
      { apiName: this.apiName, ...config },
    );

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductDto>(
      {
        method: 'GET',
        headers: { Accept: 'application/json' },
        url: `/api/catalog/products/${id}`,
      },
      { apiName: this.apiName, ...config },
    );

  getList = (input: PagedAndSortedResultRequestDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ProductDto>>(
      {
        method: 'GET',
        headers: { Accept: 'application/json' },
        url: '/api/catalog/products',
        params: {
          sorting: input.sorting,
          skipCount: input.skipCount,
          maxResultCount: input.maxResultCount,
        },
      },
      { apiName: this.apiName, ...config },
    );

  update = (id: string, input: CreateUpdateProductDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductDto>(
      {
        method: 'PUT',
        headers: { Accept: 'application/json' },
        url: `/api/catalog/products/${id}`,
        body: input,
      },
      { apiName: this.apiName, ...config },
    );
}
