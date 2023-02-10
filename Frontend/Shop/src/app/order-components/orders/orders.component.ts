import { Component, OnInit } from '@angular/core';
import { Order } from '../../shared/dtos/order';
import { ShopAdministrationService } from '../../shared/services/shop-administration.service';
import { firstOfMonth, localDateTimeFormat, parseLocal } from '../../shared/date-functions'
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';

@Component({
  selector: 'caas-orders',
  templateUrl: './orders.component.html',
  styles: [
  ]
})
export class OrdersComponent implements OnInit {
  searchString : string = '';
  dateFrom: Date = firstOfMonth();
  dateTo: Date = new Date();
  viewOrders: Order[] = [];
  orders: Order[] = [];
  pageIdx: number = 0;
  maxPageIdx: number = 0;
  itemsPerPage: number = 20;
  loading: boolean = false;

  constructor(
    private adminService: ShopAdministrationService,
    private route: ActivatedRoute,
    private location: Location
  ){}


  applyFilter(pageIdx: number = 0){
    this.loading = true;
    const key = String(sessionStorage.getItem('appKey'));
    this.adminService.getOrders(key, this.dateFrom, this.dateTo, this.searchString)
    .subscribe(res => {
      this.orders = res;
      this.maxPageIdx = Math.floor((this.orders.length) / this.itemsPerPage);
      if(this.orders.length % this.itemsPerPage === 0 && this.maxPageIdx > 0)
        this.maxPageIdx--;
      this.pageChange(pageIdx);
      this.loading = false;
    });
  }

  currentRoute(){
    return `admin/orders?from=${
      localDateTimeFormat(this.dateFrom)
    }&to=${
      localDateTimeFormat(this.dateTo)
    }&pattern=${
      this.searchString
    }&pageIdx=${
      this.pageIdx
    }`;
  }

  dateValue(d: Date): string{
    return localDateTimeFormat(d);
  }

  startChange(s: string){
    this.dateFrom = parseLocal(s);
  }

  endChange(s: string){
    this.dateTo = parseLocal(s);
  }

  pageChange(pageIdx: number){
    if(pageIdx < 0)
      this.pageIdx = 0;
    else if(pageIdx > this.maxPageIdx)
      this.pageIdx = this.maxPageIdx;
    else
      this.pageIdx = pageIdx;
    this.viewOrders = this.orders.slice(
      this.pageIdx * this.itemsPerPage,
      this.pageIdx * this.itemsPerPage + this.itemsPerPage
    );
    this.location.replaceState(this.currentRoute());
  }

  ngOnInit(): void {
    this.loading = true;
    this.route.queryParamMap.subscribe((params) => {
      this.searchString = params.get('pattern') ?? '';
      if(params.has('to')) {
        this.dateTo = this.dateTo = parseLocal(params.get('to'));
      }
      if(params.has('from')) {
        this.dateFrom = this.dateFrom = parseLocal(params.get('from'));
      }
      this.applyFilter(Number(params.get('pageIdx') ?? '0'));
    });
  }
}
