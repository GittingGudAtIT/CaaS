import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { environment } from 'src/environments/environment';
import { Customer } from '../shared/dtos/customer';
import { CartManagementService } from '../shared/services/cart-management.service';
import { DiscountManagementService } from '../shared/services/discount-management.service';

class ErrorMessage{
  constructor(
    public forControl: string,
    public forValidator: string,
    public text: string
  ){}
}

@Component({
  selector: 'caas-checkout',
  templateUrl: './checkout.component.html',
  styles: [
  ]
})
export class CheckoutComponent implements OnInit {
  checkoutForm! : FormGroup;
  errors: {[key: string]: string} = {};

  constructor(
    private fb: FormBuilder,
    private cartService: CartManagementService,
    private router: Router
  ){}

  private emailPattern = '[a-zA-Z0-9\.]{3,}@[a-zA-Z0-9]{2,}\.[a-z]{2,6}';
  ngOnInit(): void {
    this.checkoutForm = this.fb.group({
      firstName: ['first name', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]],
      lastName: ['last name', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]],
      email: ['some.ones@example.com', [Validators.required, Validators.pattern(this.emailPattern), Validators.maxLength(255)]],
    });
    this.checkoutForm.statusChanges.subscribe(() => { this.updateErrors();});
  }

  submit(){
    const id = localStorage.getItem(`${environment.shopId}_cartId`) ?? '';
    localStorage.removeItem(`${environment.shopId}_cartId`);

    this.cartService.checkout(id, new Customer(
      this.checkoutForm.value.firstName,
      this.checkoutForm.value.lastName,
      this.checkoutForm.value.email
    )).subscribe(res => {
      if(res && res['id']){
        this.router.navigate(['order/' + res['id']], {queryParams: { returnUrl: '/home'}});
      } else alert('checkout failed');
    })
  }

  private errorMessages = [
    new ErrorMessage('firstName', 'required', 'fist name is required'),
    new ErrorMessage('lastName', 'required', 'last name is required'),
    new ErrorMessage('email', 'required', 'email is required'),
    new ErrorMessage('firstName', 'minlength', 'first name must have at least 3 characters'),
    new ErrorMessage('lastName', 'minlength', 'last name must have at least 3 characters'),
    new ErrorMessage('email', 'pattern', `email must match the following pattern: \'${this.emailPattern}\'`),
  ]

  updateErrors(){
    this.errors = {};

    for (const message of this.errorMessages) {
      const control = this.checkoutForm.get(message.forControl);
      if (control &&
          control.dirty &&
          control.invalid &&
          control.errors != null &&
          control.errors[message.forValidator] &&
          !this.errors[message.forControl]) {
        this.errors[message.forControl] = message.text;
      }
    }
  }
}
