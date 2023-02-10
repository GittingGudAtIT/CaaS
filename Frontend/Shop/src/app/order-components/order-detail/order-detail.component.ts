import { DatePipe } from '@angular/common';
import { Component, Input, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Order } from '../../shared/dtos/order';
import { ShopAdministrationService } from '../../shared/services/shop-administration.service';


@Component({
  selector: 'caas-order-detail',
  templateUrl: './order-detail.component.html',
  styles: [
  ]
})
export class OrderDetailComponent implements OnInit {
  order: Order = new Order();
  entriesExpanded: boolean = true;

  returnUrl: string = '/home';
  params: any;
  found: boolean = true;
  invalidId: string = '';

  constructor(
    private adminService: ShopAdministrationService,
    private route: ActivatedRoute,
    public datePipe: DatePipe,
  ){}

  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      if(params['returnUrl'])
        this.returnUrl = params['returnUrl'];

      this.params = {...params};
      delete this.params['returnUrl'];
    });

    const id = this.route.snapshot.params['id'];
    this.adminService.getOrder(id)
    .subscribe(res => {
      if(res && res['id']){
        this.order = res;
      } else {
        this.invalidId = id;
        this.found = false;
      }
    });
  }
}
