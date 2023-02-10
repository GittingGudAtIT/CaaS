import { DiscountWoProducts } from "./discount-wo-products";


export class DiscountLookup{
  constructor(
    public discount: DiscountWoProducts = new DiscountWoProducts(),
    public productIds: string[] = []
  ){}
}
