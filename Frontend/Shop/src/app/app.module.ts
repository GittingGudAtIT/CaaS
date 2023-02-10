import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { HomeComponent } from './home/home.component';
import { ProductsComponent } from './product-components/products/products.component';

import { HttpClientModule } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule }  from '@angular/forms';
import { OAuthModule } from 'angular-oauth2-oidc';
import { LoginComponent } from './admin-components/login/login.component';
import { ShopComponent } from './admin-components/shop/shop.component';
import { DiscountsComponent } from './admin-components/discount-components/discounts/discounts.component';
import { OrdersComponent } from './order-components/orders/orders.component';
import { StatisticsComponent } from './admin-components/statistic-components/statistics/statistics.component';
import { CartComponent } from './cart/cart.component';
import { OrderComponent } from './order-components/order/order.component';
import { OrderDetailComponent } from './order-components/order-detail/order-detail.component';
import { DatePipe } from '@angular/common';
import { ListNavigatorComponent } from './shared/components/list-navigator/list-navigator.component';
import { ProductDetailComponent } from './product-components/product-detail/product-detail.component';
import { ProductComponent } from './product-components/product/product.component';
import { AmountControlComponent } from './shared/components/amount-control/amount-control.component';
import { DiscountComponent } from './admin-components/discount-components/discount/discount.component';
import { FreeprodViewComponent } from './admin-components/discount-components/freeprod-view/freeprod-view.component';
import { DiscountDetailComponent } from './admin-components/discount-components/discount-detail/discount-detail.component';
import { FreeprodEditComponent } from './admin-components/discount-components/freeprod-edit/freeprod-edit.component';
import { ProductSimpleDeleteComponent } from './admin-components/discount-components/product-simple-delete/product-simple-delete.component';
import { ProductSelectViewComponent } from './product-components/product-select-view/product-select-view.component';
import { ProductSelectViewItemComponent } from './product-components/product-select-view-item/product-select-view-item.component';
import { ProductSelectModalComponent } from './product-components/product-select-modal/product-select-modal.component';
import { ErrorsComponent } from './shared/components/errors/errors.component';
import { OrderEntryComponent } from './order-components/order-entry/order-entry.component';
import { ProductDetailAdminComponent } from './product-components/product-detail-admin/product-detail-admin.component';
import { ProductDetailCreateComponent } from './product-components/product-detail-create/product-detail-create.component';
import { AverageCartCountComponent } from './admin-components/statistic-components/average-cart-count/average-cart-count.component';
import { SalesComponent } from './admin-components/statistic-components/sales/sales.component';
import { TopsellersComponent } from './admin-components/statistic-components/topsellers/topsellers.component';
import { BalkChartComponent } from './shared/components/balk-chart/balk-chart.component';
import { CheckoutComponent } from './checkout/checkout.component';
import { DiscountUserViewComponent } from './admin-components/discount-components/discount-user-view/discount-user-view.component';


@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    ProductsComponent,
    LoginComponent,
    ShopComponent,
    DiscountsComponent,
    OrdersComponent,
    StatisticsComponent,
    CartComponent,
    OrderComponent,
    OrderDetailComponent,
    ListNavigatorComponent,
    ProductDetailComponent,
    ProductComponent,
    AmountControlComponent,
    DiscountComponent,
    FreeprodViewComponent,
    DiscountDetailComponent,
    FreeprodEditComponent,
    ProductSimpleDeleteComponent,
    ProductSelectViewComponent,
    ProductSelectViewItemComponent,
    ProductSelectModalComponent,
    ErrorsComponent,
    OrderEntryComponent,
    ProductDetailAdminComponent,
    ProductDetailCreateComponent,
    AverageCartCountComponent,
    SalesComponent,
    TopsellersComponent,
    BalkChartComponent,
    CheckoutComponent,
    DiscountUserViewComponent,
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    NgbModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    OAuthModule.forRoot(),
  ],
  providers: [DatePipe],
  bootstrap: [AppComponent]
})
export class AppModule { }
