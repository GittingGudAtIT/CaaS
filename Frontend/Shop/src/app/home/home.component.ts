import { Component, OnInit } from '@angular/core';
import { Discount } from '../shared/dtos/discount';
import { DiscountLookup } from '../shared/dtos/discount-lookup';
import { DiscountWoProducts } from '../shared/dtos/discount-wo-products';
import { Product } from '../shared/dtos/product';
import { ProductAmount } from '../shared/dtos/product-amount';
import { DiscountManagementService } from '../shared/services/discount-management.service';
import { ProductManagementService } from '../shared/services/product-management.service';
import { ShopAdministrationService } from '../shared/services/shop-administration.service';

@Component({
  selector: 'caas-home',
  templateUrl: './home.component.html',
  styles: [
  ]
})
export class HomeComponent implements OnInit {
  shopName : string = '';
  topSellers: ProductAmount[] = [];
  activeDiscounts: Discount[] = [];
  loading: boolean = true;
  private discountLuts: DiscountLookup[] = [];

  constructor(
    private shopService: ShopAdministrationService,
    private productService: ProductManagementService,
    private discountService: DiscountManagementService
  ){}

  productsWithDiscounts() : {product: Product, discounts: DiscountWoProducts[]} []{
    return this.topSellers.map(pa => { return {
      product: pa.product,
      discounts: this.discountLuts
        .filter(lut => lut.productIds.indexOf(pa.product.id) >= 0)
          .map(lut => lut.discount)
      }
    });
  }

  ngOnInit(): void {
    this.shopService.getShop()
      .subscribe(shop => {
        if(shop && shop['name']) this.shopName = shop.name;
      });
    this.shopService.getTopSellers(new Date(new Date().setFullYear(2000)), new Date(Date.now()), 6)
      .subscribe(topsellers => {
        if(topsellers && topsellers instanceof Array<ProductAmount>) {
          this.topSellers = topsellers;
          this.productService.getDiscountsForProducts(this.topSellers.map(pa => pa.product.id))
            .subscribe(luts => {
              if(luts && luts instanceof Array<DiscountLookup>){
                this.discountLuts = luts;
              } else this.discountLuts = [];
            })
        }
        this.loading = false;
      });
    this.discountService.getActiveDiscounts()
      .subscribe(discounts => {
        if(discounts && discounts instanceof Array<Discount>){
          this.activeDiscounts = discounts;
        }
      });

  }

}
