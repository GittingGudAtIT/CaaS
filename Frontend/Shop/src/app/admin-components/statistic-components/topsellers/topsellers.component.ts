import { Component, Input, OnInit, Output } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { firstOfYear, localDateTimeFormat, parseLocal } from 'src/app/shared/date-functions';
import { ProductAmount } from 'src/app/shared/dtos/product-amount';
import { ShopAdministrationService } from 'src/app/shared/services/shop-administration.service';
import { Location } from '@angular/common';

@Component({
  selector: 'caas-topsellers',
  templateUrl: './topsellers.component.html',
  styles: [
  ]
})
export class TopsellersComponent implements OnInit {
  loading: boolean = true;
  dateFrom: Date = firstOfYear();
  dateTo: Date = new Date();
  @Input() pageIdx : number = 0;
  count: number = 20;
  topsellers: ProductAmount[] = [];

  constructor(
    private shopService: ShopAdministrationService,
    private route: ActivatedRoute,
    private location: Location
  ){}

  params(): any{
    return {
      returnUrl: '/admin/statistics',
      from: localDateTimeFormat(this.dateFrom),
      to: localDateTimeFormat(this.dateTo),
      count: this.count,
      pageIdx: this.pageIdx
    }
  }

  updateUrl(){
    this.location.replaceState(
      `admin/statistics?pageIdx=${
        this.pageIdx
      }&from=${
        localDateTimeFormat(this.dateFrom)
      }&to=${
        localDateTimeFormat(this.dateTo)
      }&count=${
        this.count
      }`
    );
  }

  startChange(s: string){
    this.dateFrom = parseLocal(s);
  }

  endChange(s: string){
    this.dateTo = parseLocal(s);
  }

  amountChanged(n: number){
    this.count = n;
  }

  update(){
    this.shopService.getTopSellers(this.dateFrom, this.dateTo, this.count)
    .subscribe(res => {
      this.loading = false;
      if(res){
        this.topsellers = res;
      }
    });
  }

  ngOnInit(): void {
    this.route.queryParamMap.subscribe(params => {
      if(params.has('pageIdx') && this.pageIdx === Number(params.get('pageIdx'))){
        if(params.has('to')) {
          this.dateTo = this.dateTo = parseLocal(params.get('to'));
        }
        if(params.has('from')) {
          this.dateFrom = this.dateFrom = parseLocal(params.get('from'));
        }
        if(params.has('count')){
          this.count = Number(params.get('count'));
        }
        this.updateUrl();
      }
      this.update();
    });
  }
}
