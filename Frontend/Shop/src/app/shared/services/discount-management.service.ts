import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, of } from 'rxjs';
import { environment } from 'src/environments/environment';
import { localDateTimeFormat } from '../date-functions';
import { Discount } from '../dtos/discount';

@Injectable({
  providedIn: 'root'
})
export class DiscountManagementService {

  constructor(
    private httpClient: HttpClient
  ) { }

  private errorHandler(error: Error | any | null): Observable<any> {
    console.log(error);
    return of(error);
  }

  getDiscount(id: string): Observable<Discount | null>{
    return this.httpClient.get<Discount | null>(`${environment.api}/discounts/${id}`)
      .pipe(catchError(this.errorHandler))
  }

  getDiscounts(pattern: string, from: Date, to: Date): Observable<Discount[]>{
    return this.httpClient.get<Discount[]>(`${environment.api}/discounts?pattern=${pattern}&from=${
        localDateTimeFormat(from)
      }&to=${localDateTimeFormat(to)}`)
      .pipe(catchError(this.errorHandler));
  }

  getActiveDiscounts(): Observable<Discount[]>{
    return this.httpClient.get<Discount[]>(`${environment.api}/discounts/allactive`)
      .pipe(catchError(this.errorHandler));
  }

  updateDiscount(appKey: string, discount: Discount): Observable<Response>{
    // fix timezone
    discount.validFrom.setMinutes(discount.validFrom.getMinutes() - discount.validTo.getTimezoneOffset());
    discount.validTo.setMinutes(discount.validTo.getMinutes() - discount.validTo.getTimezoneOffset());

    return this.httpClient.put<Response>(`${environment.api}/discounts/${discount.id?? ''}`,
      {discount: discount, appKey: appKey},
      {observe: 'response'}).pipe(catchError(this.errorHandler));
  }

  createDiscount(appKey: string, discount: Discount): Observable<Discount> {
    // fix timezone
    discount.validFrom.setMinutes(discount.validFrom.getMinutes() - discount.validTo.getTimezoneOffset());
    discount.validTo.setMinutes(discount.validTo.getMinutes() - discount.validTo.getTimezoneOffset());

    return this.httpClient.post<Discount>(`${environment.api}/discounts`, {discount: discount, appKey: appKey})
      .pipe(catchError(this.errorHandler));
  }

  deleteDiscount(appKey: string, id: string): Observable<Response> {
    const options = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      }),
      body: {
        key: appKey
      }
    };
    return this.httpClient.request<Response>('delete', `${environment.api}/discounts/${id}`, options)
      .pipe(catchError(this.errorHandler));
  }
}
