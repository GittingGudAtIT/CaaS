import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { firstOfMonth } from '../../shared/date-functions';
import { Shop } from '../../shared/dtos/shop';
import { ShopAdministrationService } from '../../shared/services/shop-administration.service';


@Component({
  selector: 'caas-shop',
  templateUrl: './shop.component.html',
  styles: [
  ]
})

export class ShopComponent {

  shop: Shop = new Shop();
  sales: Number = 0;

  editingName: boolean = false;
  newName: string = '';

  editingAppKey: boolean = false;
  key: string = '';
  keyConfirmed = '';

  keysEqual: boolean = true;


  constructor(private auth: AuthenticationService, private router: Router, private adminService: ShopAdministrationService) {
    this.adminService.getShop().subscribe(
      res => this.shop = res
    );
    this.adminService.getSales({
      from: firstOfMonth(),
      to: new Date(),
      appKey: String(sessionStorage.getItem('appKey'))
    }).subscribe(res => typeof res === 'number' ? this.sales = res : 0)
  }

  logout(){
    this.router.navigate(['/home']).then(() =>{
      sessionStorage.removeItem('appKey');
      this.auth.logout();
    });
  }

  editNameClick(){
    this.editingName = !this.editingName;
  }

  editAppKeyClick(){
    this.editingAppKey = !this.editingAppKey;
  }

  confirmNameClick(){
    if(!this.auth.isLoggedIn()){
      alert('An error occured during name change.');
    } else {
      const key = String(sessionStorage.getItem('appKey'));
      this.adminService.updateShop(key, key, this.newName)
      .subscribe(response => {
        if(response.status === 200){
          this.shop!.name = this.newName;
          this.newName = '';
          this.editingName = false;
        } else {
          alert('An error occured during name change.');
        }
      });
    }
  }

  confirmAppKeyClick(){
    if(!this.auth.isLoggedIn()){
      alert('An error occured during app key change');
    } else {
      const key = String(sessionStorage.getItem('appKey'));
      this.adminService.updateShop(key, this.key, this.shop!.name!)
      .subscribe(response => {
        if(response.status === 200){
          sessionStorage.setItem('appKey', key);
          this.key = '';
          this.keyConfirmed = '';
          this.editingAppKey = false;
          alert('App key has been changed.');
        } else{
          alert('An error occured during app key change');
        }
      })
    }
  }

  keyChange(){
    this.keysEqual = this.key === this.keyConfirmed;
  }


}
