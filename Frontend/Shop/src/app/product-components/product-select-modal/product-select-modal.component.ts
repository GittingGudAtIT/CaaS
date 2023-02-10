import { AfterViewChecked, Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Observable } from 'rxjs';

export class ModalCloseEvent{
  constructor(
    public result: ModalResult,
    public selectedIds: Set<string>
  ){}
}


export enum ModalResult{
  Ok,
  Cancel,
  Close
}

@Component({
  selector: 'caas-product-select-modal',
  templateUrl: './product-select-modal.component.html',
  styles: [
  ]
})

export class ProductSelectModalComponent {
  @Input() updateContentEventCall = new EventEmitter<void>();
  @Input() title: string = 'no title';
  @Input() open: boolean = false;
  @Input() excludedProductIds = () => new Set<string>();
  @Output() onCloseEvent = new EventEmitter<ModalCloseEvent>();

  private selectedItems = new Set<string>();

  onModalClose() {
    this.open = false;
    this.onCloseEvent.emit(new ModalCloseEvent(ModalResult.Close, this.selectedItems));
  }

  onModalCancel(){
    this.open = false;
    this.onCloseEvent.emit(new ModalCloseEvent(ModalResult.Cancel, this.selectedItems));
  }

  onModalSubmit(){
    this.open = false;
    this.onCloseEvent.emit(new ModalCloseEvent(ModalResult.Ok, this.selectedItems));
  }

  selectedItemsChanged(selection: Set<string>){
    this.selectedItems = selection;
  }
}
