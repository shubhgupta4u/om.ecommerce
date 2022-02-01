import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { NgxAccountLibraryModule,JwtInterceptor } from 'ngx-account-library';
import { ProductModule } from '../domain_modules/product/product.module';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    ProductModule,
    NgxAccountLibraryModule.forRoot({
      authProvider:2, // 1- NativeWebForm, 2- Okta and 3- AzureAD
      nativeAuthApiConfig:
      {
        loginUri:"https://localhost:44386/api/Auth/token",
        logoutUri:"https://localhost:44386/api/Auth/logout",
        refreshUri:"https://localhost:44386/api/Auth/refresh",
        loginRedirectRoute:"product",
        logoutRedirectRoute:"account",
        authScope:"om.ecommerce"
      },
      oktaAuthConfig:
      {
        clientId:"0oa3rqwjeikxFYVOY5d7",
        issuer:"https://dev-35555547.okta.com/oauth2/default",
        logoutRedirectUri:"http://localhost:4200/account/logout/callback",
        loginRedirectUri:"http://localhost:4200/account/login/callback",
      }
    })
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true }
],
  bootstrap: [AppComponent]
})
export class AppModule { }
