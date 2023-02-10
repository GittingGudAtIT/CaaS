import { DiscountLookup } from "./discount-lookup";
import { DiscountWoProducts } from "./discount-wo-products";

export class CartDiscounts{
  constructor(
    public productDiscounts?: DiscountLookup[],
    public valueDiscounts?: DiscountWoProducts[]
  ){}
}
