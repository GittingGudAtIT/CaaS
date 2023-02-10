import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ProductAmount } from 'src/app/shared/dtos/product-amount';

@Component({
  selector: 'caas-freeprod-edit',
  templateUrl: './freeprod-edit.component.html',
  styles: [
  ]
})
export class FreeprodEditComponent {
  @Input() productAmount: ProductAmount = new ProductAmount();
  @Output() deleteClickEvent = new EventEmitter<void>();

  amountChanged(value: number){
    this.productAmount.count = value;
  }

  deleteClick(){
    this.deleteClickEvent.emit();
  }
}
