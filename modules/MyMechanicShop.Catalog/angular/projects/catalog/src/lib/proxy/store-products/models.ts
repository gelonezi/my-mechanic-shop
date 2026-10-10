import type { Currency } from '../my-mechanic-shop/shared-kernel/enums/currency.enum';
import type { EntityDto } from '@abp/ng.core';
import type { ProductUnit } from '../my-mechanic-shop/shared-kernel/enums/product-unit.enum';

export interface ActivateStoreProductDto extends StorePriceDto {
  productId?: string;
}

export interface StorePriceDto {
  priceAmount?: number;
  priceCurrency?: Currency;
}

export interface StoreProductDto extends EntityDto<string> {
  productId?: string;
  productName?: string;
  productBrand?: string | null;
  productUnit?: ProductUnit;
  priceAmount?: number;
  priceCurrency?: Currency;
  isActive?: boolean;
}
