import { MinType } from "../enums/min-type";
import { OffType } from "../enums/off-type";
import { ProductAmount } from "./product-amount";

export class Discount{
  constructor(
    public id: string = '',
    public offType: OffType = OffType.None,
    public offValue: number = 0,
    public description: string = '',
    public tag: string = '',
    public minType: MinType = MinType.ProductCount,
    public minValue: number = 0,
    public is4AllProducts: boolean = false,
    public freeProducts: ProductAmount[] = [],
    public products: string[] = [],
    public validFrom: Date = new Date(Date.now()),
    public validTo: Date = new Date(Date.now())
  ){}
}
