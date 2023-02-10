import { Component, Input, OnInit } from '@angular/core';
import { DiscountWoProducts } from 'src/app/shared/dtos/discount-wo-products';
import { Product } from '../../shared/dtos/product';

@Component({
  selector: 'caas-product',
  templateUrl: './product.component.html',
  styles: [
  ]
})
export class ProductComponent {
  @Input() product: Product = new Product();
  @Input() discounts: DiscountWoProducts[] = [];
}
