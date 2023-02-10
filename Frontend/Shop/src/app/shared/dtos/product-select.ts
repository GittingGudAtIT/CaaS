import { Product } from "./product";

export class ProductSelect{
  constructor(
    public product: Product = new Product(),
    public checked: boolean = false
  ){}
}
