import { DatePipe } from '@angular/common';
import { Component, Input } from '@angular/core';
import { DiscountWoProducts } from 'src/app/shared/dtos/discount-wo-products';

@Component({
  selector: 'caas-discount-user-view',
  templateUrl: './discount-user-view.component.html',
  styles: [
  ]
})
export class DiscountUserViewComponent {
  @Input() discount: DiscountWoProducts = new DiscountWoProducts();
}
