import { mapEnumToOptions } from '@abp/ng.core';

export enum Currency {
  Undefined = 0,
  Usd = 840,
  Eur = 978,
  Brl = 986,
}

export const currencyOptions = mapEnumToOptions(Currency);
