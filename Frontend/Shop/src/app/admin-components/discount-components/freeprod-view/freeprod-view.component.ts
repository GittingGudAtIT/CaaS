import { Component, Input } from '@angular/core';
import { ProductAmount } from 'src/app/shared/dtos/product-amount';

@Component({
  selector: 'caas-freeprod-view',
  templateUrl: './freeprod-view.component.html',
  styles: [
  ]
})
export class FreeprodViewComponent {
  @Input() productAmount: ProductAmount = new ProductAmount();
}
