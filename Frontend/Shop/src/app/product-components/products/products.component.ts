import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, defaultUrlMatcher } from '@angular/router';
import { Product } from '../../shared/dtos/product';
import { ProductManagementService } from '../../shared/services/product-management.service';
import { Location } from '@angular/common';
import { AuthenticationService } from 'src/app/shared/services/authentication.service';
import { DiscountWoProducts } from 'src/app/shared/dtos/discount-wo-products';
import { DiscountLookup } from 'src/app/shared/dtos/discount-lookup';

class ProductDiscounts{
  constructor(
    public product: Product,
    public discounts: DiscountWoProducts[] = []
  ){}
}

@Component({
  selector: 'caas-products',
  templateUrl: './products.component.html',
  styles: [
  ]
})

export class ProductsComponent implements OnInit {
  loading: boolean = false;
  searchString: string = '';
  pageIdx: number = 0;
  maxPageIdx: number = 0;
  itemsPerPage: number = 20;
  isAdmin: boolean = false;

  products: Product[] = [];
  viewProducts: ProductDiscounts[] = [];

  constructor(
    private productService: ProductManagementService,
    private route: ActivatedRoute,
    private location: Location,
    public auth: AuthenticationService
  ){}

  applyFilter(pageIdx: number = 0){
    this.loading = true;
    this.productService.getProducts(this.searchString)
    .subscribe(res => {
      this.products = res;
      this.maxPageIdx = Math.floor((this.products.length) / this.itemsPerPage);
      if(this.products.length % this.itemsPerPage === 0 && this.maxPageIdx > 0)
        this.maxPageIdx--;
      this.pageChange(pageIdx);
      this.loading = false;
    });
  }

  currentRoute(){
    return `${this.isAdmin ? '/admin': ''}/products?pattern=${
      this.searchString
    }&pageIdx=${
      this.pageIdx
    }`;
  }

  pageChange(pageIdx: number = 0){
    if(pageIdx < 0)
      this.pageIdx = 0;
    else if(pageIdx > this.maxPageIdx)
      this.pageIdx = this.maxPageIdx;
    else
      this.pageIdx = pageIdx;

    this.viewProducts = this.products.slice(
      this.pageIdx * this.itemsPerPage,
      this.pageIdx * this.itemsPerPage + this.itemsPerPage
    ).map(p => new ProductDiscounts(p));

    this.location.replaceState(this.currentRoute());
    this.loadDiscounts();
  }

  private loadDiscounts() {
    const ids = this.viewProducts.map(p => p.product.id);
    this.productService.getDiscountsForProducts(ids)
      .subscribe(res => {
        if(res){
          this.viewProducts.forEach(vp => {
            res.forEach(dlu => {
              if(dlu.productIds.indexOf(vp.product.id) > -1)
                vp.discounts.push(dlu.discount);
            })
          })
        }
      });
  }

  queryParams(){
    return {
      pattern: this.searchString,
      pageIdx: this.pageIdx
    }
  }

  ngOnInit(): void {
    this.isAdmin = this.route.parent?.snapshot.url.filter(seg => seg.path === 'admin').length === 1;
    this.loading = true;
    this.route.queryParamMap.subscribe((params) => {
      this.searchString = params.get('pattern') ?? '';
      this.applyFilter(Number(params.get('pageIdx') ?? '0'));
    });
  }
}
