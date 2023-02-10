import { Customer } from "./customer";
import { OrderEntry } from "./order-entry";

export class Order{
  constructor(
    public id: string = '',
    public dateTime: Date = new Date(Date.now()),
    public offSum: number = 0,
    public customer: Customer = new Customer(),
    public entries: OrderEntry[] = [],
    public total: number = 0,
    public downloadLink: string = ''
  ){}
}
