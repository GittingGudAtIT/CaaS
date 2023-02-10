import { ProductAmount } from "./product-amount";

export class Cart {
  constructor(
    public id: string = '',
    public entries: ProductAmount[] = []
  ){}
}
