import { Component, Inject, OnInit, Optional } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AccountModuleConfig, AuthProvider } from '../../interfaces/ngx-account-module-config';
import { AccountFacadeService } from '../../services/account-facade.service';
import { ACCOUNT_MODULE_CONFIG_TOKEN } from '../../../public-api';

@Component({
  selector: 'lib-logout',
  templateUrl: './logout.component.html',
  styleUrls: ['./logout.component.css']
})
export class LogoutComponent implements OnInit {

  private _accountModuleConfig: AccountModuleConfig;

  constructor(private accountService: AccountFacadeService,
              @Optional() @Inject(ACCOUNT_MODULE_CONFIG_TOKEN)
              private readonly config: AccountModuleConfig | null,
              private route: ActivatedRoute) { 
                if (config != null) {
                  this._accountModuleConfig = config;
                }
              }

  ngOnInit(): void {
    if(this._accountModuleConfig && this._accountModuleConfig.authProvider == AuthProvider.NativeWebForm){
      this.logoutNativeWebform();
    }
    else if(this._accountModuleConfig && this._accountModuleConfig.authProvider == AuthProvider.Okta){
      this.logoutOkta();
    }   
  }

  logoutOkta(){
    this.accountService.oktaLogout();
    this.logoutNativeWebform();
  }

  logoutNativeWebform(){
    const returnUrl = this.route.snapshot.queryParams['returnUrl'];
    this.accountService.logout(returnUrl).subscribe(()=>{
      console.log("logout successfully")
    },error=>{
      console.error("Error occured while logout")
      console.error(error);
    })
  }
}
