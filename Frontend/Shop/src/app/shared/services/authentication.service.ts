import { Injectable, } from '@angular/core';
import { OAuthService,  OAuthEvent } from 'angular-oauth2-oidc';


@Injectable({
  providedIn: 'root'
})
export class AuthenticationService {

  constructor(private oauthService: OAuthService) { }

  login() {
    this.oauthService.initCodeFlow();
  }

  logout(){
    this.oauthService.logOut(true);
  }

  isLoggedIn() {
    return this.oauthService.hasValidAccessToken() && this.oauthService.hasValidIdToken();
  }
}
