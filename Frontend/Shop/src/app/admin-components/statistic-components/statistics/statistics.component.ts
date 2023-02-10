import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouteConfigLoadEnd } from '@angular/router';
import { ShopAdministrationService } from 'src/app/shared/services/shop-administration.service';

@Component({
  selector: 'caas-statistics',
  templateUrl: './statistics.component.html',
  styles: [
  ]
})
export class StatisticsComponent implements OnInit {
  pageIdx: number = 0;
  params: any;

  constructor(
    private route: ActivatedRoute
  ){}

  setPage(idx: number){
    this.pageIdx = idx;
  }


  ngOnInit(): void {
    this.route.queryParamMap.subscribe(params => {
      if(params.has('pageIdx')){
        this.pageIdx = Number(params.get('pageIdx'));
      }
    });
  }
}
