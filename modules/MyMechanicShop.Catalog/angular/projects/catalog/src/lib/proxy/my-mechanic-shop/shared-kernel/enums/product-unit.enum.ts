import { mapEnumToOptions } from '@abp/ng.core';

export enum ProductUnit {
  Undefined = 0,
  Unit = 1,
  Pair = 2,
  Kit = 3,
  Box = 4,
  Package = 5,
  Gram = 10,
  Kilogram = 11,
  Milliliter = 20,
  Liter = 21,
  CubicMeter = 22,
  Centimeter = 30,
  Meter = 31,
  SquareMeter = 40,
}

export const productUnitOptions = mapEnumToOptions(ProductUnit);
