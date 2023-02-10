import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Order } from '../../shared/dtos/order';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'caas-order',
  templateUrl: './order.component.html',
  styles: [
  ]
})
export class OrderComponent {
  constructor(public datePipe: DatePipe){}
  @Input() order: Order = new Order();
  @Input() parentPath: string = '/';
}
