import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, map, Observable, of } from 'rxjs';
import { environment } from 'src/environments/environment';
import { localDateTimeFormat } from '../date-functions';
import { Order } from '../dtos/order';
import { ProductAmount } from '../dtos/product-amount';
import { Shop } from '../dtos/shop';
import { TimeRangeRequest } from '../dtos/time-range-request';
import { TopsellerRequest } from '../dtos/topseller-request';
import { WeekdayDistribution } from '../dtos/weekday-distribution';

@Injectable({
  providedIn: 'root'
})
export class ShopAdministrationService {

  constructor(private httpClient: HttpClient) { }

  private adminPath: string = environment.api + '/administration';
  private statisticPath: string = this.adminPath + '/statistics';

  private errorHandler(error: Error | any): Observable<any> {
    console.log(error);
    return of(error);
  }

  getShop(): Observable<Shop>{
    return this.httpClient.get<Shop>(`${environment.api}`)
      .pipe(catchError(this.errorHandler));
  }

  getTopSellers(from: Date, to: Date, count: number): Observable<ProductAmount[]>{
    return this.httpClient.post<ProductAmount[]>(`${environment.api}/topsellers`, {
      from: localDateTimeFormat(from),
      to: localDateTimeFormat(to),
      count: count
    }).pipe(catchError(this.errorHandler));
  }

  getOrder(id: string){
    return this.httpClient.get<Order>(`${environment.api}/orders/${id}`)
      .pipe(catchError(this.errorHandler));
  }

  getOrders(appKey: string, from: Date, to: Date, searchTerm: string){
    return this.httpClient.post<Order[]>(`${this.adminPath}/orders?from=${localDateTimeFormat(from)}&to=${localDateTimeFormat(to)}&pattern=${searchTerm}`, {key: appKey})
      .pipe(catchError(this.errorHandler));
  }

  checkAppKey(appKey: string): Observable<boolean>{
    return this.httpClient.post<boolean>(`${this.adminPath}/login`, {key: appKey})
      .pipe(catchError(this.errorHandler));
  }

  updateShop(oldAppKey: string, newAppKey: string, newName: string): Observable<Response>{
    return this.httpClient.put<Response>(
      `${this.adminPath}`,
      {appkey: oldAppKey, shop: {appKey: newAppKey, name: newName} },
      {observe: 'response'}
    ).pipe(catchError(this.errorHandler));
  }

  deleteShop(appKey: string): Observable<any>{
    const options = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      }),
      body: {
        key: appKey
      }
    };
    return this.httpClient.delete<any>(`${environment.api}`, options)
      .pipe(catchError(this.errorHandler));
  }

  getSales(request: TimeRangeRequest): Observable<Number>{
    return this.httpClient.post<any>(`${this.statisticPath}/sales`, request)
      .pipe(map<any, Number>(res => res), catchError(this.errorHandler));
  }

  getCartSalesDistribution(request: TimeRangeRequest): Observable<WeekdayDistribution<Number>>{
    return this.httpClient.post<WeekdayDistribution<Number>>(`${this.statisticPath}/cartsalesdistributed`, request)
      .pipe(catchError(this.errorHandler));
  }

  getCartCountsDistribution(request: TimeRangeRequest): Observable<WeekdayDistribution<Number>>{
    return this.httpClient.post<WeekdayDistribution<Number>>(`${this.statisticPath}/cartcountsdistributed`, request)
      .pipe(catchError(this.errorHandler));
  }
}
