import { Component, EventEmitter, Input, Output } from '@angular/core';
import { AbstractControl, ControlValueAccessor, NG_VALIDATORS, NG_VALUE_ACCESSOR, ValidationErrors, Validator } from '@angular/forms';
import { numberFieldKeyDown } from '../../number-input';

@Component({
  selector: 'caas-amount-control',
  templateUrl: './amount-control.component.html',
  styles: [
  ],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      multi: true,
      useExisting: AmountControlComponent
    },
    {
      provide: NG_VALIDATORS,
      multi: true,
      useExisting: AmountControlComponent
    }
  ]
})
export class AmountControlComponent implements ControlValueAccessor, Validator{
  private oldAmount: number = 0;

  onChange = (amount: number) => {};
  onTouched = () => {};

  touched = false;
  disabled = false;

  @Input() amount: number = 0;
  @Input() min: number = 1;
  @Input() max: number = Number.MAX_SAFE_INTEGER;
  @Input() allowDecimal: boolean = false;
  @Input() notenabled = false;
  @Output() valueChangedEvent = new EventEmitter<number>();


  inc(){
    this.markAsTouched();
    if(this.amount <= this.max -1){
      this.amount += 1;
    } else if(this.amount < this.max){
      this.amount = this.max;
    }
    this.onChange(this.amount);
    this.valueChangedEvent.emit(this.amount);
  }

  dec(){
    this.markAsTouched();
    if(this.amount >= this.min + 1){
      this.amount -= 1;
    } else if(this.amount > this.min){
      this.amount = this.min;
    }
    this.onChange(this.amount);
    this.valueChangedEvent.emit(this.amount);
  }

  onKeyDown(e: KeyboardEvent){
    // blocks keys
    numberFieldKeyDown(e, this.allowDecimal);
  }

  onValueChanged(e: any){
    const val = Number(e);
    this.markAsTouched();
    if(!isFinite(val) || isNaN(val)){
      this.amount = this.oldAmount;
    } else{
      this.amount = val;
      this.oldAmount = this.amount;
    }
    this.onChange(this.amount);
    this.valueChangedEvent.emit(this.amount);
  }

  writeValue(value: number): void {
    this.amount = value;
    this.valueChangedEvent.emit(this.amount);
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState(disabled: boolean){
    this.notenabled = disabled;
  }

  markAsTouched(){
    if(!this.touched){
      this.onTouched();
      this.touched = true;
    }
  }

  validate(control: AbstractControl<any, any>): ValidationErrors | null {
    const amount = control.value;
    if(amount < this.min) {
      return {
        valueToSmall: {
          amount
        }
      }
    } else if(String(control.value).indexOf('.') > 0 && !this.allowDecimal){
      return {
        noDecimalPoint: {
          amount
        }
      }
    } else if(amount > this.max) {
      return {
        valueToBig: {
          amount
        }
      }
    }
    else return null;
  }
}
