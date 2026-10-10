import type { ProductUnit } from '../my-mechanic-shop/shared-kernel/enums/product-unit.enum';
import type { EntityDto } from '@abp/ng.core';

export interface CreateUpdateProductDto {
  name: string;
  brand?: string | null;
  ean?: string | null;
  description?: string | null;
  unit?: ProductUnit;
}

export interface ProductDto extends EntityDto<string> {
  name?: string;
  brand?: string | null;
  ean?: string | null;
  description?: string | null;
  unit?: ProductUnit;
}
