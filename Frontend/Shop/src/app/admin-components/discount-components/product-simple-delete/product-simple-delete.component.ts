import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'caas-product-simple-delete',
  templateUrl: './product-simple-delete.component.html',
  styles: [
  ]
})
export class ProductSimpleDeleteComponent {
  @Input() id: string = '';
  @Input() name: string = '';
  @Output() deleteClickEvent = new EventEmitter<string>();

  deleteClick(){
    this.deleteClickEvent.emit(this.id);
  }
}
