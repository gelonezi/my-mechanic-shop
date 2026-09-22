
export interface CreateUpdateProductDto {
  name: string;
  price: number;
  stockCount?: number;
}

export interface ProductDto {
  id?: string;
  name?: string;
  price?: number;
  stockCount?: number;
}
