import { Component, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ActivatedRoute, Router, TitleStrategy } from '@angular/router';
import { Cart } from 'src/app/shared/dtos/cart';
import { DiscountWoProducts } from 'src/app/shared/dtos/discount-wo-products';
import { ProductAmount } from 'src/app/shared/dtos/product-amount';
import { numberFieldKeyDown } from 'src/app/shared/number-input';
import { CartManagementService } from 'src/app/shared/services/cart-management.service';
import { environment } from 'src/environments/environment';
import { Product } from '../../shared/dtos/product';
import { ProductManagementService } from '../../shared/services/product-management.service';

@Component({
  selector: 'caas-product-detail',
  templateUrl: './product-detail.component.html',
  styles: [
  ]
})
export class ProductDetailComponent implements OnInit {
  product: Product = new Product();
  discounts: DiscountWoProducts[] = [];
  returnUrl: string = '/products';
  params : any;
  count: number = 1;
  alreadyAdded: boolean = false;
  lastCnt: number = 1;

  @ViewChild('addToCartForm') addToCartForm!: NgForm;


  constructor(
    private productService : ProductManagementService,
    private route: ActivatedRoute,
    private router: Router,
    private cartService: CartManagementService
  ){}

  inc(){
    this.count++;
    this.alreadyAdded = false;
  }

  dec(){
    if(this.count > 1)
      this.count--;
    this.alreadyAdded = false;
  }

  onKeyDown(e: KeyboardEvent){
    numberFieldKeyDown(e);
    this.alreadyAdded = false;
  }

  submitForm(){
    const s = localStorage.getItem(`${environment.shopId}_cartId`);
    this.lastCnt = this.count;

    // update
    if(s){
      // parse
      this.cartService.getCart(s).subscribe(cart => {
        if(cart){

          // check if alredy exists
          if(!cart.entries)
            cart.entries = [];
          const item = cart.entries.find(p => p.product?.id === this.product.id);

          // increase
          if(item){
            item.product = this.product; // update because why not
            item.count = this.count + item.count;
          } else{
            // add new entry
            cart.entries.push(new ProductAmount(this.product, this.count));
          }

          this.cartService.updateCart(cart).subscribe(r => {
            if(r.ok){
              this.alreadyAdded = true;
            } else {
              alert('something wen\'t wrong during update.');
            }
          })
        }
      });
    // create new
    } else{
      // new cartentry
      const cart = new Cart('', [
        new ProductAmount(this.product, this.count)
      ]);
      this.cartService.createCart(cart).
        subscribe(c => {
          const s = c['id'];
          if(s){
            localStorage.setItem(`${environment.shopId}_cartId`, s);
            this.alreadyAdded = true;
          } else {
            alert('something wen\'t wrong during create.');
          }
        })
    }
  }

  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      const s = params['returnUrl'];
      if(s) this.returnUrl = s;
      this.params = {...params};
      delete this.params['returnUrl'];
    });

    const id = this.route.snapshot.params['id'];
    this.productService.getProduct(id)
    .subscribe(res => {
      this.product = res;
      if(!this.product['id']) {
        this.product = new Product();
        alert("could not find product");
      }
    });

    this.productService.getDiscountsForProduct(id)
    .subscribe(res => {
      if(res) this.discounts = res;
    })
  }
}
