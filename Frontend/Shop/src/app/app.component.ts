import { Component, HostListener } from '@angular/core';
import { Router } from '@angular/router';
import { OAuthService } from 'angular-oauth2-oidc';
import { JwksValidationHandler } from 'angular-oauth2-oidc-jwks';
import { authConfig } from './auth.config';

@Component({
  selector: 'caas-root',
  templateUrl: './app.component.html',
  styles: []
})
export class AppComponent {
  title = 'Shop';
  dropdownVisible: boolean = false;

  constructor(private oauthService: OAuthService, private router: Router){
    this.configureWithNewConfigApi();
  }

  @HostListener('document:click', ['$event'])
  someWhereClick(event: MouseEvent){
    if(event.target instanceof HTMLElement){
      const elem = event.target as HTMLElement;
      const attr = elem.attributes.getNamedItem('id');
      if((attr?.value !== 'admin-button'))
        this.hideDropdown();
    }
  }

  private configureWithNewConfigApi() {
    this.oauthService.configure(authConfig);
    this.oauthService.tokenValidationHandler = new JwksValidationHandler();
    this.oauthService.loadDiscoveryDocumentAndTryLogin();
  }

  anyAdminChildActive() : boolean{
    return this.router.url.startsWith('/admin')
  }

  dropDownClick(){
    this.dropdownVisible = !this.dropdownVisible;
  }

  hideDropdown(){
    this.dropdownVisible = false;
  }

  windowWidth(): number{
    return window.innerWidth;
  }

  shopActive(): boolean{
    return this.router.url.startsWith('/admin/shop');
  }

  discountActive(): boolean{
    return this.router.url.startsWith('/admin/discounts');
  }

  productsActive(): boolean{
    return this.router.url.startsWith('/admin/products');
  }

  ordersActive(): boolean{
    return this.router.url.startsWith('/admin/orders');
  }

  statisticsActive(): boolean{
    return this.router.url.startsWith('/admin/statistics');
  }
}
