import { Component, NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CanNavigateToAdminGuard } from './shared/can-navigate-to-admin.guard';
import { CartComponent } from './cart/cart.component';
import { DiscountsComponent } from './admin-components/discount-components/discounts/discounts.component';
import { HomeComponent } from './home/home.component';
import { LoginComponent } from './admin-components/login/login.component';
import { OrderDetailComponent } from './order-components/order-detail/order-detail.component';
import { OrdersComponent } from './order-components/orders/orders.component';
import { ProductDetailComponent } from './product-components/product-detail/product-detail.component';
import { ProductsComponent } from './product-components/products/products.component';
import { ShopComponent } from './admin-components/shop/shop.component';
import { StatisticsComponent } from './admin-components/statistic-components/statistics/statistics.component';
import { DiscountDetailComponent } from './admin-components/discount-components/discount-detail/discount-detail.component';
import { ProductDetailAdminComponent } from './product-components/product-detail-admin/product-detail-admin.component';
import { ProductDetailCreateComponent } from './product-components/product-detail-create/product-detail-create.component';
import { CheckoutComponent } from './checkout/checkout.component';


const routes: Routes = [
  {
    path: '',
    redirectTo: 'home',
    pathMatch: 'full'
  },
  {
    path: 'home',
    component: HomeComponent
  },
  {
    path: 'cart',
    component: CartComponent
  },
  {
    path: 'products',
    component: ProductsComponent,
  },
  {
    path: 'products/:id',
    component: ProductDetailComponent
  },
  {
    path: 'login',
    component: LoginComponent
  },
  {
    path: 'checkout',
    component: CheckoutComponent
  },
  {
    path: 'order/:id',
    component: OrderDetailComponent
  },
  {
    path: 'admin',
    children:[
      {
        path: 'shop',
        component: ShopComponent,
        canActivate: [CanNavigateToAdminGuard]
      },
      {
        path: 'discounts',
        component: DiscountsComponent,
        canActivate: [CanNavigateToAdminGuard]
      },
      {
        path: 'discounts/:id',
        component: DiscountDetailComponent,
        canActivate: [CanNavigateToAdminGuard]
      },
      {
        path: 'discounts/new',
        component: DiscountDetailComponent,
        canActivate: [CanNavigateToAdminGuard]
      },
      {
        path: 'products',
        component: ProductsComponent,
        canActivate: [CanNavigateToAdminGuard]
      },
      {
        path: 'products/new',
        component: ProductDetailCreateComponent,
        canActivate: [CanNavigateToAdminGuard]
      },
      {
        path: 'products/:id',
        component: ProductDetailAdminComponent,
        canActivate: [CanNavigateToAdminGuard]
      },
      {
        path: 'orders',
        component: OrdersComponent,
        canActivate: [CanNavigateToAdminGuard]
      },
      {
        path: 'orders/:id',
        component: OrderDetailComponent,
      },
      {
        path: 'statistics',
        component: StatisticsComponent,
        canActivate: [CanNavigateToAdminGuard]
      }
    ]
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
