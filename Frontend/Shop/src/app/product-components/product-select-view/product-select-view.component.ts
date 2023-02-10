import { AfterViewChecked, Component, EventEmitter, Input, OnDestroy, OnInit, Output } from '@angular/core';
import { Product } from '../../shared/dtos/product';
import { ProductSelect } from '../../shared/dtos/product-select';
import { ProductManagementService } from '../../shared/services/product-management.service';

@Component({
  selector: 'caas-product-select-view',
  templateUrl: './product-select-view.component.html',
  styles: [
  ]
})
export class ProductSelectViewComponent implements OnInit, OnDestroy {
  @Input() excludedProductIds = () => new Set<string>();
  @Input() updateEventCall = new EventEmitter<void>();
  @Output() selectedItemsChangeEvent = new EventEmitter<Set<string>>();

  loading: boolean = false;
  searchString: string = '';
  pageIdx: number = 0;
  maxPageIdx: number = 0;
  itemsPerPage: number = 10;
  allSelected: boolean = false;

  products: Product[] = [];
  filteredProducts: Product[] = [];
  selectedItems = new Set<string>();

  viewProducts(): ProductSelect[]{
    return this.filteredProducts
      .map(p => new ProductSelect(p, this.selectedItems.has(p.id)));
  }

  constructor(
    private productService : ProductManagementService,
  ){}


  ngOnDestroy(): void {
    this.updateEventCall.unsubscribe();
  }

  ngOnInit(): void {
    // reset can be called from parent
    this.updateEventCall.subscribe(() => {
      this.applyFilter()
      this.selectedItems.clear();
      this.allSelected = false;
    });
    
    this.productService.getProducts('')
      .subscribe(res => {
        this.products = res;
        this.applyFilter();
      });
  }

  selectionAll(){
    this.allSelected = !this.allSelected;
    if(this.allSelected){
      this.filteredProducts.forEach(p => {
        if(!this.selectedItems.has(p.id))
          this.selectedItems.add(p.id);
      });
    } else {
      this.filteredProducts.forEach(p => {
        if(this.selectedItems.has(p.id))
          this.selectedItems.delete(p.id);
      });
    }
    this.selectedItemsChangeEvent.emit(this.selectedItems);
  }

  clearClick(){
    this.allSelected = false;
    this.selectedItems.clear();
    this.selectedItemsChangeEvent.emit(this.selectedItems);
  }

  applyFilter(){
    this.loading = true;
    this.productService.getProducts(this.searchString)
    .subscribe(res => {
      const set = this.excludedProductIds();
      this.products = res.filter(p => !set.has(p.id));
      this.maxPageIdx = Math.floor((this.products.length) / this.itemsPerPage);
      if(this.products.length % this.itemsPerPage === 0 && this.maxPageIdx > 0)
        this.maxPageIdx--;
      this.pageChange();
      this.loading = false;
    });
  }

  checkedChange(e: ProductSelect){
    if(e.checked){
      this.selectedItems.add(e.product.id);
      this.selectedItemsChangeEvent.emit(this.selectedItems);
    } else {
      this.selectedItems.delete(e.product.id);
      this.selectedItemsChangeEvent.emit(this.selectedItems);
    }
    this.allSelected = this.filteredProducts.every(p => this.selectedItems.has(p.id));
  }

  pageChange(pageIdx: number = 0){
    if(pageIdx < 0)
      this.pageIdx = 0;
    else if(pageIdx > this.maxPageIdx)
      this.pageIdx = this.maxPageIdx;
    else
      this.pageIdx = pageIdx;
    this.filteredProducts = this.products.slice(
      this.pageIdx * this.itemsPerPage,
      this.pageIdx * this.itemsPerPage + this.itemsPerPage
    );
    this.allSelected = this.filteredProducts.every(p => this.selectedItems.has(p.id));
  }
}
