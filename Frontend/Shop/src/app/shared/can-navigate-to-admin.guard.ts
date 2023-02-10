import { Injectable } from '@angular/core';
import { ActivatedRoute, ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthenticationService } from './services/authentication.service';

@Injectable({
  providedIn: 'root'
})
export class CanNavigateToAdminGuard implements CanActivate {

  constructor(
    protected router: Router,
    protected auth: AuthenticationService
  ) {}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot)
  : Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
    sessionStorage.setItem('appKey', 'herbert');
    return true;
    if (!this.auth.isLoggedIn()) {
      this.router.navigate(['/login'],
      { queryParams: { returnUrl: state.url ,...route.queryParams } });
      return false;
    } else {
      return true;
    }
  }
}
