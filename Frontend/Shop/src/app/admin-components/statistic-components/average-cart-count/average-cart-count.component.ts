import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { firstOfYear, localDateTimeFormat, parseLocal } from 'src/app/shared/date-functions';
import { Location } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { ShopAdministrationService } from 'src/app/shared/services/shop-administration.service';
import { WeekdayDistribution } from 'src/app/shared/dtos/weekday-distribution';
import { TimeRangeRequest } from 'src/app/shared/dtos/time-range-request';

@Component({
  selector: 'caas-average-cart-count',
  templateUrl: './average-cart-count.component.html',
  styles: [
  ]
})
export class AverageCartCountComponent implements OnInit {
  dateFrom: Date = firstOfYear();
  dateTo: Date = new Date();
  @Input() pageIdx : number = 0;
  private key: string = '';
  weekdayDistribution = new WeekdayDistribution<number>();

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
    this.shopService.getCartCountsDistribution(new TimeRangeRequest(this.key, this.dateFrom, this.dateTo))
      .subscribe(dis => {
        if(dis)
          this.weekdayDistribution = dis;
      })
  }

  ngOnInit(): void {
    this.key = sessionStorage.getItem('appKey') ?? '';
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
}
