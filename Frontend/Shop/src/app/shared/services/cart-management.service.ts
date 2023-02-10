import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, map, Observable, of } from 'rxjs';
import { environment } from 'src/environments/environment';
import { Cart } from '../dtos/cart';
import { CartDiscounts } from '../dtos/cart-discounts';
import { Customer } from '../dtos/customer';
import { Discount } from '../dtos/discount';
import { Order } from '../dtos/order';

@Injectable({
  providedIn: 'root'
})
export class CartManagementService {

  constructor(
    private httpClient: HttpClient
    ) { }

  getCart(id: string): Observable<Cart | null>{
    return this.httpClient.get<Cart | null>(`${environment.api}/cart/${id}`)
      .pipe(catchError(this.errorHandler));
  }

  createCart(cart: Cart): Observable<any | Cart>{
    return this.httpClient.post<Cart>(`${environment.api}/cart`, cart)
      .pipe(catchError(this.errorHandler));
  }

  updateCart(cart: Cart): Observable<Response>{
    return this.httpClient.put<Response>(`${environment.api}/cart/${cart.id}`, cart, { observe: 'response'})
      .pipe(catchError(this.errorHandler));
  }

  deleteCart(id: string): Observable<any>{
    return this.httpClient.delete<any>(`${environment.api}/cart/${id}`)
      .pipe(catchError(this.errorHandler));
  }

  getCartSum(id: string): Observable<number>{
    return this.httpClient.get<number>(`${environment.api}/cart/${id}/sum`)
      .pipe(catchError(this.errorHandler));
  }

  checkout(id: string, customer: Customer): Observable<Order | null>{
    return this.httpClient.post<Order | null>(`${environment.api}/cart/${id}/checkout`, customer)
      .pipe(catchError(this.errorHandler));
  }

  getCartDiscounts(id: string): Observable<CartDiscounts>{
    return this.httpClient.get<CartDiscounts>(`${environment.api}/cart/${id}/discounts`);
  }

  private errorHandler(error: Error | any | null): Observable<any> {
    console.log(error);
    return of(error);
  }
}

