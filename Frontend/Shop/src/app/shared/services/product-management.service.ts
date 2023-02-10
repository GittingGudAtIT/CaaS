import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, of } from 'rxjs';
import { environment } from 'src/environments/environment';
import { DiscountLookup } from '../dtos/discount-lookup';
import { DiscountWoProducts } from '../dtos/discount-wo-products';
import { Product } from '../dtos/product';
import { ProductAdmin } from '../dtos/product-admin';

@Injectable({
  providedIn: 'root'
})
export class ProductManagementService {

  constructor(
    private httpClient: HttpClient
  ){}

  private errorHandler(error: Error | any): Observable<any> {
    console.log(error);
    return of(error);
  }

  getProduct(id: string): Observable<Product>{
    return this.httpClient.get<Product>(`${environment.api}/products/${id}`)
      .pipe(catchError(this.errorHandler));
  }

  getProductAdmin(id: string, appKey: string){
    return this.httpClient.post<Product>(`${environment.api}/products/${id}/admin`, { key: appKey})
      .pipe(catchError(this.errorHandler));
  }

  getProducts(pattern: string): Observable<Product[]>{
    return this.httpClient.get<Product[]>(`${environment.api}/products?pattern=${pattern}`)
      .pipe(catchError(this.errorHandler));
  }

  getProductRange(ids: string[]): Observable<Product[]>{
    return this.httpClient.post<Product[]>(`${environment.api}/products/range`,  ids)
      .pipe(catchError(this.errorHandler));
  }

  getDiscountsForProducts(ids: string[]): Observable<DiscountLookup[]>{
    return this.httpClient.post<DiscountLookup[]>(`${environment.api}/products/discountsforrange`, ids)
      .pipe(catchError(this.errorHandler));
  }

  getDiscountsForProduct(id: string): Observable<DiscountWoProducts[]>{
    return this.httpClient.get<DiscountWoProducts[]>(`${environment.api}/products/${id}/discounts`)
      .pipe(catchError(this.errorHandler));
  }

  updateProduct(id: string, price: number, appKey: string): Observable<Response>{
    return this.httpClient.put<Response>(`${environment.api}/products/${id}`, { appKey: appKey, price: price }, {observe: 'response'})
      .pipe(catchError(this.errorHandler));
  }

  createProduct(product: ProductAdmin, appKey: string): Observable<Product>{
    return this.httpClient.post<ProductAdmin>(`${environment.api}/products`, { product: product, appKey: appKey})
      .pipe(catchError(this.errorHandler));
  }

  deleteProduct(id: string, appKey: string): Observable<Response>{
    const options = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      }),
      body: {
        key: appKey
      }
    };
    return this.httpClient.delete<Response>(`${environment.api}/products/${id}`, options)
      .pipe(catchError(this.errorHandler));
  }
}
