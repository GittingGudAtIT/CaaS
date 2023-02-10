import { Component, Input, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { OrderEntry } from 'src/app/shared/dtos/order-entry';
import { Product } from 'src/app/shared/dtos/product';
import { ProductAmount } from 'src/app/shared/dtos/product-amount';

@Component({
  selector: 'caas-order-entry',
  templateUrl: './order-entry.component.html',
  styles: [
  ]
})
export class OrderEntryComponent implements OnInit {
  @Input() entry: OrderEntry | undefined;
  @Input() orderId: string | undefined;
  expanded: boolean = true;

  returnUrl: string = '/home';
  params: any;

  constructor(
    private route: ActivatedRoute
  ){}

  ngOnInit(): void {
    if(this.route.snapshot.url.filter(seg => seg.path === 'admin').length === 1){
      this.returnUrl = '/admin/orders/' + this.orderId;
    } else {
      this.returnUrl = '/orders/' + this.orderId;
    }
    this.route.queryParams.subscribe(params => {
      this.params = {...params};
      if(params['returnUrl'])
        this.returnUrl = params['returnUrl'];
      delete this.params['returnUrl'];
    })
  }
}
