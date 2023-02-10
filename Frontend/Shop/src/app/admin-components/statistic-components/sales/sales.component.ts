import { Component, Input, OnInit } from '@angular/core';
import { firstOfYear, localDateTimeFormat, parseLocal } from 'src/app/shared/date-functions';
import { Location } from '@angular/common'
import { ActivatedRoute } from '@angular/router';
import { WeekdayDistribution } from 'src/app/shared/dtos/weekday-distribution';
import { ShopAdministrationService } from 'src/app/shared/services/shop-administration.service';
import { TimeRangeRequest } from 'src/app/shared/dtos/time-range-request';

@Component({
  selector: 'caas-sales',
  templateUrl: './sales.component.html',
  styles: [
  ]
})
export class SalesComponent implements OnInit {
  loading: boolean = true;
  dateFrom: Date = firstOfYear();
  dateTo: Date = new Date();
  @Input() pageIdx : number = 0;
  private key: string = '';


  value: number = 0;
  weekdayDistributionSales = new WeekdayDistribution<number>();

  constructor(
    private location: Location,
    private route: ActivatedRoute,
    private shopService: ShopAdministrationService
  ){}


  startChange(s: string){
    this.dateFrom = parseLocal(s);
  }

  endChange(s: string){
    this.dateTo = parseLocal(s);
  }

  ngOnInit(): void {
    this.key = sessionStorage.getItem('appKey')?? '';
    this.route.queryParamMap.subscribe(params => {
      if(params.has('pageIdx') && this.pageIdx === Number(params.get('pageIdx'))){
        if(params.has('to')) {
          this.dateTo = this.dateTo = parseLocal(params.get('to'));
        }
        if(params.has('from')) {
          this.dateFrom = this.dateFrom = parseLocal(params.get('from'));
        }
        this.updateUrl();
      }
      this.update();
    });
  }

  updateUrl(){
    this.location.replaceState(
      `admin/statistics?pageIdx=${
        this.pageIdx
      }&from=${
        localDateTimeFormat(this.dateFrom)
      }&to=${
        localDateTimeFormat(this.dateTo)
      }`
    );
  }

  update(){
    var value = false;
    var sales = false;

    const req = new TimeRangeRequest(this.key, this.dateFrom, this.dateTo);
    this.shopService.getSales(req)
      .subscribe(res => {
        if(res && typeof res === 'number'){
          this.value = res;
          value = true;
          if(value) this.loading = false;
        } else this.value = 0;
    })
    this.shopService.getCartSalesDistribution(req)
      .subscribe(res => {
        if(res){
          sales = true;
          this.weekdayDistributionSales = res;
          if(value) this.loading = false;
        }
      });
  }

}
