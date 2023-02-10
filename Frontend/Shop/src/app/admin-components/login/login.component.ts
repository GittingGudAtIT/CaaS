import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { ShopAdministrationService } from '../../shared/services/shop-administration.service';

@Component({
  selector: 'caas-login',
  templateUrl: './login.component.html',
  styles: [
  ]
})
export class LoginComponent implements OnInit, OnDestroy {
  appKey: string = '';
  appKeyWrong: boolean = false;
  pending: boolean = false;
  private returnUrl: string = 'home';


  constructor(
    private auth: AuthenticationService,
    private adminService: ShopAdministrationService,
    private route: ActivatedRoute,
    private router: Router
  ){ }

  async pollLoginAsync(): Promise<boolean> {
    for(var i = 0; i < 50; ++i){
      if(!this.pending)
        return false;
      if(this.auth.isLoggedIn()){
        this.pending = false;
        this.router.navigateByUrl(this.returnUrl);
        return true;
      }
      await new Promise(f => setTimeout(f, 100));
    }
    this.pending = false;
    this.router.navigateByUrl('home');
    return false;
  }


  ngOnDestroy(): void {
    this.pending = false;
  }

  ngOnInit(): void {
    // comes back from auth
    const url = sessionStorage.getItem('requestedPage');
    if(url){
      this.returnUrl = url;
      sessionStorage.removeItem('requestedPage');
      this.pending = true;
      this.pollLoginAsync();
    // wants to open admin page first time
    } else{
      this.route.queryParams
      .subscribe(params => {
        const s = params['returnUrl'];
        if(s)
          this.returnUrl = s;
      });
    }
  }

  submitForm() {
    const key = this.appKey;
    this.adminService.checkAppKey(this.appKey)
    .subscribe(res => {
      if(res){
        sessionStorage.setItem('appKey', key);
        sessionStorage.setItem('requestedPage', this.returnUrl);
        this.auth.login();
      } else{
        this.appKeyWrong = true;
      }
    });
  }
}
