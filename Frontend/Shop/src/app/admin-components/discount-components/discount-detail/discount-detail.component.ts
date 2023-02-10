import { Component, EventEmitter, OnInit } from '@angular/core';
import { AbstractControl,  CheckboxControlValueAccessor,  ControlContainer,  FormBuilder, FormControlStatus, FormGroup, ValidatorFn, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ModalCloseEvent, ModalResult } from 'src/app/product-components/product-select-modal/product-select-modal.component';
import { localDateTimeFormat } from 'src/app/shared/date-functions';
import { evaluateConfigMessage } from 'src/app/shared/discount-functions';
import { Discount } from 'src/app/shared/dtos/discount';
import { Product } from 'src/app/shared/dtos/product';
import { ProductAmount } from 'src/app/shared/dtos/product-amount';
import { MinType, minTypeLabelMapping, minTypeValues } from 'src/app/shared/enums/min-type';
import { OffType, offTypeLabelMapping, offTypeValues } from 'src/app/shared/enums/off-type';
import { DiscountManagementService } from 'src/app/shared/services/discount-management.service';
import { ProductManagementService } from 'src/app/shared/services/product-management.service';

@Component({
  selector: 'caas-discount-detail',
  templateUrl: './discount-detail.component.html',
  styles: [
  ]
})
export class DiscountDetailComponent implements OnInit {

  discountForm!: FormGroup;
  errors : { [key: string] : string[]} | null = null;

  returnUrl: string = '/admin/discounts';
  params: any;
  discount: Discount = new Discount();

  minTypeMapping = minTypeLabelMapping;
  minTypes = minTypeValues;
  offTypeMapping = offTypeLabelMapping;
  offTypes = offTypeValues;
  affectedModalOpen = false;
  giftModalOpen = false;

  createNewDiscount: boolean = false;
  submited: boolean = false;
  wasNew: boolean = false;

  products: Product[] = [];
  created: boolean = false;

  giftModalUpdateEvent = new EventEmitter<void>();
  productModalUpdateEvent = new EventEmitter<void>();

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private discountService: DiscountManagementService,
    private productService: ProductManagementService,
    private formBuilder: FormBuilder
  ){}

  private updateDiscount(){
    this.discount.is4AllProducts = Boolean(this.discountForm.value.is4AllProducts);
    this.discount.minType = Number(this.discountForm.value.minType);
    this.discount.minValue = Number(this.discountForm.value.minValue);
    this.discount.offType = Number(this.discountForm.value.offType);
    this.discount.offValue = Number(this.discountForm.value.offValue);
    this.discount.validFrom = new Date(this.discountForm.value.validFrom);
    this.discount.validTo = new Date(this.discountForm.value.validTo);
    this.discount.description = String(this.discountForm.value.description);
    this.discount.tag = String(this.discountForm.value.tag);

    if(OffType[this.discount.offType] === OffType[OffType.Percentual]){
      this.discount.offValue /= 100;
    }
  }

  submitForm(){
    this.updateDiscount();

    if(this.discount.is4AllProducts)
      this.discount.products = [];

    const key = String(sessionStorage.getItem('appKey'));

    if(this.createNewDiscount){
      this.discount.id = '';
      this.discountService.createDiscount(key, this.discount)
      .subscribe(d => {
        if(d){
          this.discount = d;
          this.submited = true;
          this.created = true;
          this.createNewDiscount = false;
          this.wasNew = true;
        }
      })
    } else {
      this.discountService.updateDiscount(key, this.discount)
      .subscribe(response => {
        if(!response.ok){
          alert('updating discount failed');
        } else {
          this.submited = true;
          this.wasNew = false;
        }
      });
    }

  }

  disabledCheckbox(): boolean{
    return this.discount.freeProducts.length === 0
      || MinType[this.discountForm.value.minType] === MinType[MinType.CartSum];
  }

  deleteClick(){
    const key = sessionStorage.getItem('appKey')?? '';
    this.discountService.deleteDiscount(key, this.discount.id)
      .subscribe(() => {
        this.router.navigate([this.returnUrl], { queryParams: this.params});
      }
    );
  }


  clearGifts(){
    this.discount.freeProducts = [];
    this.updateErrors();
  }

  clearProducts(){
    this.products = [];
    this.discount.products = [];
    this.updateErrors();
  }

  excludedProducts = () => {
    return new Set<string>(this.discount.products);
  }

  excludedGifts = () => {
    return new Set<string>(this.discount.freeProducts.map(pa => pa.product.id));;
  }

  private compareByName(a: Product, b: Product): number{
    if(a.name < b.name)
    return -1;
    if(a.name > b.name)
      return 1;
    return 0;
  }

  updateErrors(){

    this.submited = false;
    const fv = this.discountForm.value;
    this.errors = null;

    if(new Date(fv.validTo) <= new Date(Date.now())){
      if(!this.errors) this.errors = {};
      this.errors['date'] = [];
      this.errors['date'].push(
        'Valid until date can\'t be before now otherwhise this discount would never active.'
      );
    }

    if(new Date(fv.validFrom) >= new Date(fv.validTo)){
      if(!this.errors) this.errors = {};
      if(!this.errors['offValue']) this.errors['date'] = [];
      this.errors['date'].push(
        'Valid until date has to be greater than valid from date:'
      );
    }

    if(OffType[fv.offType] !== OffType[OffType.None] && Number(fv.offValue) <= 0){
      if(!this.errors) this.errors = {};
      this.errors['offValue'] = [];
      this.errors['offValue'].push(
        'Off Value can\'t be zero.'
      );
    }

    if(OffType[fv.offType] === OffType[OffType.Percentual] && Number(fv.offValue) > 100){
      if(!this.errors) this.errors = {};
      if(!this.errors['offValue'])this.errors['offValue'] = [];
      this.errors['offValue'].push(
        'You can\'t give more than 100% off.'
      );
    }

    if(OffType[fv.offType] === OffType[OffType.None]  && this.discount.freeProducts.length < 1){
      if(!this.errors) this.errors = {};
      if(!this.errors['offValue'])this.errors['offValue'] = [];
      this.errors['offValue'].push(
        'If Off Type \'None\' is selected, there should be at least one Gift configured'
      );
    }

    if(MinType[fv.minType] === MinType[MinType.ProductCount]
      && !Boolean(fv.is4AllProducts) && this.discount.products.length < 1){
      if(!this.errors) this.errors = {};
      if(!this.errors['offValue']) this.errors['offValue'] = [];
      this.errors['offValue'].push(
        'If Off Type \'Product Count\' is choosen, you must configure Affected Products.'
      );
    }

    if(MinType[fv.minType] === MinType[MinType.CartSum] && OffType[fv.offType] === OffType[OffType.FreeProduct]){
      if(!this.errors) this.errors = {};
      if(!this.errors['offValue']) this.errors['offValue'] = [];
      this.errors['offValue'].push(
        'Free products can only be defined for type \'Product Count\''
        + 'because they always refer to the product itself.'
      );
    }
  }

  backClick(){
    this.router.navigateByUrl(this.returnUrl);
  }

  doesMessage(): string{
    return evaluateConfigMessage(this.discount);
  }

  giftDelete(productAmount: ProductAmount){
    this.discount.freeProducts = this.discount.freeProducts
      .filter(pa => pa.product?.id !== productAmount.product?.id);
    this.giftModalUpdateEvent.emit();
    this.updateErrors();
  }

  affectedDeleteClick(id: string){
    this.discount.products = this.discount.products.filter(pid => pid !== id);
    this.products = this.products.filter(p => p.id !== id);
    this.giftModalUpdateEvent.emit();
    this.updateErrors();
  }

  affectedModalClosed(e: ModalCloseEvent){
    this.affectedModalOpen = false;

    if(e.result === ModalResult.Ok){
      e.selectedIds.forEach(id => this.discount.products.push(id));
      this.discount.products = this.discount.products.sort();

      this.productService.getProductRange(this.discount.products).subscribe(res => {
        this.products = res.sort();
        this.productModalUpdateEvent.emit();
        this.updateErrors();
      });
    }
  }

  giftModalClosed(e: ModalCloseEvent){
    this.giftModalOpen = false;

    if(e.result === ModalResult.Ok){
      const ids = new Array<string>();
      e.selectedIds.forEach(id => ids.push(id));

      this.productService.getProductRange(ids).subscribe(res =>{

        this.discount.freeProducts = this.discount.freeProducts
          .concat(res.map(p => new ProductAmount(p, 1)))
          .sort((a, b) => this.compareByName(a.product, b.product)
        );
        this.giftModalUpdateEvent.emit();
        this.updateErrors();
      });
    }
  }

  offIsNone(): boolean{
    return OffType[this.discountForm.value.offType]
      === OffType[OffType.None]
  }

  minTypeChanged(){
    const value = MinType[this.discountForm.value.minType];
    if(value === MinType[MinType.ProductCount]){
      this.discountForm.patchValue({minValue: Math.trunc(Number(this.discountForm.value.minValue))});
    } else if(value === MinType[MinType.CartSum]) {
      this.discountForm.patchValue({is4AllProducts: false});
    }
  }

  minValueMin(): number {
    return MinType[this.discountForm.value.minType] === MinType[MinType.ProductCount]?
    1 : 0;
  }

  minValueAllowDecimal(): boolean{
    return MinType[this.discountForm.value.minType] === MinType[MinType.CartSum];
  }

  offTypeChanged(){
    const value = OffType[this.discountForm.value.offType];
    if(value === OffType[OffType.None]){
      this.discountForm.patchValue({offValue: 0});
    } else if(value === OffType[OffType.Percentual]){
      if(Number(this.discountForm.value.offValue) > 100)
        this.discountForm.patchValue({offValue: 100});
    }
  }

  offValueAllowDecimal(): boolean{
    return OffType[this.discountForm.value.offType] === OffType[OffType.Fixed]
      || OffType[this.discountForm.value.offType] === OffType[OffType.Percentual]
  }

  offValueMin(): number{
    return OffType[this.discountForm.value.offType] === OffType[OffType.FreeProduct] ?
      1 : 0;
  }

  offValueMax(): number{
    return OffType[this.discountForm.value.offType] === OffType[OffType.Percentual] ?
      100: Number.MAX_SAFE_INTEGER;
  }

  ngOnInit(): void {

    this.discountForm = this.formBuilder.group({
      validFrom: ['', [Validators.required]],
      validTo: ['', [Validators.required]],
      description: ['', [Validators.required, Validators.maxLength(255)]],
      tag: ['', [Validators.required, Validators.maxLength(50)]],
      minType: [1, [Validators.required] ],
      minValue: [0, [Validators.required, Validators.min(0)]],
      offType: [1, [Validators.required]],
      offValue: [1, [Validators.required, Validators.min(0)] ],
      is4AllProducts: [false, [Validators.required]],
    });

    this.route.queryParams.subscribe((params) => {
      const s = params['returnUrl'];
      if(s) this.returnUrl = s;
      this.params = {...params};
      delete this.params['returnUrl'];
    });

    // if there is no id, create new
    if(this.route.snapshot.url.filter(seg => seg.path === 'new').length === 1){
      this.createNewDiscount = true;
      this.discount = new Discount('New Discount', OffType.Percentual, 0.1, 'description', 'tag', MinType.ProductCount, 10, true);
      this.discount.validTo = new Date(new Date().getFullYear(), (new Date().getMonth() + 1) % 12 , 1)
      this.discountForm.patchValue({validTo: localDateTimeFormat(this.discount.validTo)});
      this.discountForm.patchValue({validFrom: localDateTimeFormat(this.discount.validFrom)});
      this.discountForm.patchValue({description: this.discount.description});
      this.discountForm.patchValue({tag: this.discount.tag});
      this.discountForm.patchValue({minType: this.discount.minType});
      this.discountForm.patchValue({offType: this.discount.offType});
      this.discountForm.patchValue({minValue: this.discount.minValue});
      this.discountForm.patchValue({offValue: 10});
      this.discountForm.patchValue({is4AllProducts: this.discount.is4AllProducts});



      this.discountForm.statusChanges?.subscribe(() => {
        this.updateDiscount();
        this.updateErrors();
      });

    } else {
      // load products
      this.createNewDiscount = false;
      this.discountService.getDiscount(this.route.snapshot.params['id'])
      .subscribe(res => {
        if(res) {
          this.discount = res;
          this.productService.getProductRange(this.discount.products)
            .subscribe(products => {
              this.products = products;
              this.discountForm.patchValue({validTo: localDateTimeFormat(new Date(res.validTo))});
              this.discountForm.patchValue({validFrom: localDateTimeFormat(new Date(res.validFrom))});
              this.discountForm.patchValue({description: res.description});
              this.discountForm.patchValue({tag: res.tag});
              this.discountForm.patchValue({minType: res.minType});
              this.discountForm.patchValue({offType: res.offType});
              this.discountForm.patchValue({minValue: res.minValue});
              this.discountForm.patchValue({offValue: res.offValue * (OffType[res.offType] === OffType[OffType.Percentual]? 100 : 1)});
              this.discountForm.patchValue({is4AllProducts: res.is4AllProducts});

              this.discountForm.statusChanges?.subscribe(() => {
                this.updateDiscount();
                this.updateErrors();
              });
          });
        }
      });
    }
  }
}
