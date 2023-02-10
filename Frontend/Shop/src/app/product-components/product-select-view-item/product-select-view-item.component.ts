import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Product } from 'src/app/shared/dtos/product';
import { ProductSelect } from 'src/app/shared/dtos/product-select';

@Component({
  selector: 'caas-product-select-view-item',
  templateUrl: './product-select-view-item.component.html',
  styles: [
  ]
})

export class ProductSelectViewItemComponent {
  @Input() productSelect: ProductSelect = {product: new Product(), checked: false};
  @Output() checkedChangedEvent = new EventEmitter<ProductSelect>();

  checkedChanged(){
    this.productSelect.checked = !this.productSelect.checked;
    this.checkedChangedEvent.emit(this.productSelect);
  }
}
