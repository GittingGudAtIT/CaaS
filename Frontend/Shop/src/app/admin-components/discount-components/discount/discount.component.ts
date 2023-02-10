import { DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { evaluateConfigMessage } from 'src/app/shared/discount-functions';
import { Discount } from 'src/app/shared/dtos/discount';

@Component({
  selector: 'caas-discount',
  templateUrl: './discount.component.html',
  styles: [
  ]
})
export class DiscountComponent implements OnInit {
  @Input() discount: Discount = new Discount();
  @Input() params: any;

  giftsExpanded: boolean = false;

  constructor(
    public datePipe: DatePipe,
    private route: ActivatedRoute
  ){}

  doesMessage(): string{
    return evaluateConfigMessage(this.discount);
  }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      if(params){
        this.params = params;
      }
    })
  }
}
