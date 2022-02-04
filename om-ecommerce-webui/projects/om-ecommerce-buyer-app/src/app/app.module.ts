import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { NgxAccountLibraryModule,JwtInterceptor } from 'ngx-account-library';
import { ProductModule } from '../domain_modules/product/product.module';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import * as authconfigdata from './authConfig.json';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    ProductModule,
    NgxAccountLibraryModule.forRoot(authconfigdata)
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true }
],
  bootstrap: [AppComponent]
})
export class AppModule { }
