import { Component, OnInit } from '@angular/core';
import { endWith } from 'rxjs';
import { environment } from 'src/environments/environment';
import { Cart } from '../shared/dtos/cart';
import { CartDiscounts } from '../shared/dtos/cart-discounts';
import { DiscountWoProducts } from '../shared/dtos/discount-wo-products';
import { ProductAmount } from '../shared/dtos/product-amount';
import { MinType } from '../shared/enums/min-type';
import { OffType } from '../shared/enums/off-type';
import { CartManagementService } from '../shared/services/cart-management.service';

class DiscountCount{
  constructor(
    public discount: DiscountWoProducts,
    public count: number
  ) {}
}

@Component({
  selector: 'caas-cart',
  templateUrl: './cart.component.html',
  styles: [
  ]
})
export class CartComponent implements OnInit {
  cart: Cart | null = null;
  cartDiscounts: CartDiscounts = new CartDiscounts();
  loading: boolean = false;
  total: number = 0;
  oldOrderId: string = '';

  constructor(
    private cartService: CartManagementService
  ){}

  ngOnInit(): void {
    const id = localStorage.getItem(`${environment.shopId}_cartId`);
    if(id){
      this.loading = true;
      this.cartService.getCart(id)
        .subscribe(c => {
          this.loading = false;
          this.cart = c;
          this.cartService.getCartSum(id)
            .subscribe(v => {
              this.total = v;
              this.cartService.getCartDiscounts(id)
              .subscribe(res => {
                this.cartDiscounts = res;
              });
            });
        });
    }
  }

  update(): void{
    if(this.cart){
      if(this.cart.entries.length > 0){
        this.cartService.updateCart(this.cart).subscribe(r => {
          if(!r.ok)
            alert('could not update cart');
          else{
            this.cartService.getCartSum(this.cart!.id)
              .subscribe(v => {
                this.total = v;
                this.cartService.getCartDiscounts(this.cart!.id)
                .subscribe(res => {
                  this.cartDiscounts = res;
                });
              });
          }
        });
      } else{
        this.cartService.deleteCart(this.cart.id).subscribe(() => {
          this.total = 0;
          localStorage.removeItem(`${environment.shopId}_cartId`);
        })
      }
    }
  }

  discountsForEntry(entry: ProductAmount): DiscountCount[] | null{
    if(!this.cartDiscounts.productDiscounts?.length)
      return null;

    const res = this.cartDiscounts.productDiscounts
      .filter(lookup => lookup.productIds.indexOf(entry.product.id) > -1)
      .map(lookup =>
        new DiscountCount(lookup.discount,
          lookup.discount.offType === OffType.None
          || lookup.discount.offType === OffType.Percentual
          ? 1 : Math.floor(entry.count / lookup.discount.minValue)
        )
      );
    if(res.length === 0)
      return null;
    return res;
  }

  discountsForCart(): DiscountCount[] | null{
    if(!this.cartDiscounts.valueDiscounts?.length)
      return null;

    const res = this.cartDiscounts.valueDiscounts
      .map(d =>
        new DiscountCount(d,
          d.offType === OffType.None
          || d.offType === OffType.Percentual
          ? 1 : Math.floor(this.total / d.minValue)
        )
      );
    if(res.length === 0)
      return null;
    return res;
  }

  valueChanged(x: number, entry: ProductAmount){
    entry.count = x;
    this.update();
  }

  remove(entry: ProductAmount){
    if(this.cart?.entries && entry.product?.id){
      this.cart.entries = this.cart.entries.filter(x =>
          entry.product?.id !== x.product?.id
      );
      this.update();
    }
  }
}
