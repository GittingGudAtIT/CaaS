import { Component } from '@angular/core';
import { firstOfMonth, localDateTimeFormat, parseLocal } from 'src/app/shared/date-functions';
import { Discount } from 'src/app/shared/dtos/discount';
import { DiscountManagementService } from 'src/app/shared/services/discount-management.service';
import { Location } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { FormGroup } from '@angular/forms';

@Component({
  selector: 'caas-discounts',
  templateUrl: './discounts.component.html',
  styles: [
  ]
})
export class DiscountsComponent {
  form!: FormGroup;
  searchString: string = '';

  from: Date = firstOfMonth();
  to: Date = new Date(Date.now());

  loading: boolean = false;
  pageIdx: number = 0;
  maxPageIdx: number = 0;
  discounts: Discount[] = [];
  viewDiscounts: Discount[] = [];
  itemsPerPage: number = 5;


  constructor(
    private discountService: DiscountManagementService,
    private location: Location,
    private route: ActivatedRoute
  ){}

  applyFilter(pageIdx: number = 0){
    this.loading = true;
    this.discountService.getDiscounts(this.searchString, this.from, this.to)
    .subscribe(res => {
      this.discounts = res;
      this.maxPageIdx = Math.floor((this.discounts.length) / this.itemsPerPage);
      if(this.discounts.length % this.itemsPerPage === 0 && this.maxPageIdx > 0)
        this.maxPageIdx--;
      this.pageChange(pageIdx);
      this.loading = false;
    });
  }

  currentRoute(){
    return `/admin/discounts?from=${
      localDateTimeFormat(this.from)
    }&to=${
      localDateTimeFormat(this.to)
    }&pattern=${
      this.searchString
    }&pageIdx=${
      this.pageIdx
    }`;
  }

  queryParams(){
    return {
      pattern: this.searchString,
      from: localDateTimeFormat(this.from),
      to: localDateTimeFormat(this.to),
      pageIdx: this.pageIdx,
    }
  }

  startChange(s: string){
    this.from = parseLocal(s);
  }

  endChange(s: string){
    this.to = parseLocal(s);
  }

  dateValue(d: Date): string{
    return localDateTimeFormat(d);
  }

  pageChange(pageIdx: number){
    if(pageIdx < 0)
      this.pageIdx = 0;
    else if(pageIdx > this.maxPageIdx)
      this.pageIdx = this.maxPageIdx;
    else
      this.pageIdx = pageIdx;
    this.viewDiscounts = this.discounts.slice(
      this.pageIdx * this.itemsPerPage,
      this.pageIdx * this.itemsPerPage + this.itemsPerPage
    );
    this.location.replaceState(this.currentRoute());
  }

  ngOnInit(): void {
    this.loading = true;
    this.route.queryParamMap.subscribe((params) => {
      this.searchString = params.get('pattern') ?? '';
      var pageIdx = 0;
      if(params.has('to')) {
        this.to = this.to = parseLocal(params.get('to'));
      }
      if(params.has('from')) {
        this.from = this.from = parseLocal(params.get('from'));
      }
      if(params.has('pageIdx')){
        pageIdx = Number(params.get('pageIdx'));
      }
      this.applyFilter(pageIdx);
    });
  }
}


