import { Component, OnInit } from '@angular/core';
import { AccountFacadeService } from 'ngx-account-library';
import { TokenResponse } from 'projects/ngx-account-library/src/public-api';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent  implements OnInit {
  title = 'om-ecommerce-buyer-app';
  isLogin:boolean=false;

  constructor(private readonly accountService:AccountFacadeService){

  }

  ngOnInit() {
    this.accountService.tokenResponse.subscribe((tokenRespone:TokenResponse)=>{
      if(tokenRespone && tokenRespone !== undefined && tokenRespone.jwtToken){
        this.isLogin = true;
      }else{
        this.isLogin = false;
      }
    });
  }
}
