import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DiscountWoProducts } from 'src/app/shared/dtos/discount-wo-products';
import { ProductAdmin } from 'src/app/shared/dtos/product-admin';
import { ProductManagementService } from 'src/app/shared/services/product-management.service';

@Component({
  selector: 'caas-product-detail-admin',
  templateUrl: './product-detail-admin.component.html',
  styles: [
  ]
})
export class ProductDetailAdminComponent {
  product: ProductAdmin = new ProductAdmin();
  returnUrl: string = '/admin/products';
  submited: boolean = false;
  params: any;
  initialPrice: number = 0;
  discounts: DiscountWoProducts[] = [];


  constructor(
    private route: ActivatedRoute,
    private productService: ProductManagementService,
    private router: Router
  ){}

  priceChanged(value: number){
    this.submited = false;
    this.product.price = value;
  }

  submit(){
    const key = sessionStorage.getItem('appKey')?? '';
    this.productService.updateProduct(this.product.id, this.product.price, key)
      .subscribe(res => {
        if(res.ok){
          this.submited = true;
          this.initialPrice = this.product.price;
        } else {
          alert('something wen\'t wrong during update.')
        }
      })
  }

  deleteClick(){
    const key = sessionStorage.getItem('appKey')?? '';
    this.productService.deleteProduct(this.product.id, key)
      .subscribe(() => {
          this.router.navigate([this.returnUrl], { queryParams: this.params});
      });
  }

  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      const s = params['returnUrl'];
      if(s) this.returnUrl = s;
      this.params = {...params};
      delete this.params['returnUrl'];
    });

    const key = sessionStorage.getItem('appKey');
    const id = this.route.snapshot.params['id'];
    this.productService.getProductAdmin(id, key??'')
    .subscribe(res => {
      this.product = res;
      this.initialPrice = this.product.price;
    });
    this.productService.getDiscountsForProduct(id)
    .subscribe(res => {
      if(res) this.discounts = res;
    })
  }
}
