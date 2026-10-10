import type { ActivateStoreProductDto, StorePriceDto, StoreProductDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedAndSortedResultRequestDto, PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';
import type { ProductDto } from '../products/models';

@Injectable({
  providedIn: 'root',
})
export class StoreProductsService {
  private restService = inject(RestService);
  apiName = 'Catalog';

  activate = (input: ActivateStoreProductDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StoreProductDto>(
      {
        method: 'POST',
        headers: { Accept: 'application/json' },
        url: '/api/catalog/store-products/activate',
        body: input,
      },
      { apiName: this.apiName, ...config },
    );

  deactivate = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StoreProductDto>(
      {
        method: 'POST',
        headers: { Accept: 'application/json' },
        url: `/api/catalog/store-products/${id}/deactivate`,
      },
      { apiName: this.apiName, ...config },
    );

  getAvailableProducts = (input: PagedAndSortedResultRequestDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ProductDto>>(
      {
        method: 'GET',
        headers: { Accept: 'application/json' },
        url: '/api/catalog/store-products/available-products',
        params: {
          sorting: input.sorting,
          skipCount: input.skipCount,
          maxResultCount: input.maxResultCount,
        },
      },
      { apiName: this.apiName, ...config },
    );

  getList = (input: PagedAndSortedResultRequestDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<StoreProductDto>>(
      {
        method: 'GET',
        headers: { Accept: 'application/json' },
        url: '/api/catalog/store-products',
        params: {
          sorting: input.sorting,
          skipCount: input.skipCount,
          maxResultCount: input.maxResultCount,
        },
      },
      { apiName: this.apiName, ...config },
    );

  updatePrice = (id: string, input: StorePriceDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StoreProductDto>(
      {
        method: 'PUT',
        headers: { Accept: 'application/json' },
        url: `/api/catalog/store-products/${id}/price`,
        body: input,
      },
      { apiName: this.apiName, ...config },
    );
}
