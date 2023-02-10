import { Product } from "./product";

export class ProductAmount {
  constructor(
    public product: Product = new Product(),
    public count: number = 0
  ){}
}
