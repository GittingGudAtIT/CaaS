import { OrderProduct } from "./order-product";

export class OrderEntry {
  constructor(
    public rowNumber: number = 0,
    public count: number = 0,
    public product: OrderProduct = new OrderProduct()
  ){}
}
