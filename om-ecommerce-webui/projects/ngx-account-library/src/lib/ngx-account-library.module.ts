import {  ModuleWithProviders, NgModule } from '@angular/core';
import { LoginComponent } from './components/login/login.component';
import { LogoutComponent } from './components/logout/logout.component';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { NgxAccountLibraryRoutingModule } from './ngx-account-library-routing.module';
import { AccountModuleConfig } from './interfaces/ngx-account-module-config';
import { ACCOUNT_MODULE_CONFIG_TOKEN} from '../public-api';
import { CommonModule } from '@angular/common';

import { IPublicClientApplication, PublicClientApplication, Configuration } from '@azure/msal-browser';
import { MsalGuard, MsalBroadcastService, MsalService, MSAL_INSTANCE } from '@azure/msal-angular';
import { NgxOmCommonLibraryModule } from 'ngx-om-common-library';



export const msalConfig: Configuration = {
  auth: {
    clientId: ''
  }
}


/**
 * Here we pass the configuration parameters to create an MSAL instance.
 * For more info, visit: https://github.com/AzureAD/microsoft-authentication-library-for-js/blob/dev/lib/msal-angular/docs/v2-docs/configuration.md
 */

export function MSALInstanceFactory(): IPublicClientApplication {
  return new PublicClientApplication(msalConfig);
}


@NgModule({
  declarations: [
    LoginComponent,
    LogoutComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    HttpClientModule,
    NgxAccountLibraryRoutingModule,
    NgxOmCommonLibraryModule
  ],
  providers: [
    {
      provide: MSAL_INSTANCE,
      useFactory: MSALInstanceFactory
    },
    MsalService,
    MsalGuard,
    MsalBroadcastService
  ],
  exports: [
    LoginComponent,
    LogoutComponent
  ]
})
export class NgxAccountLibraryModule {
  static forRoot(config: AccountModuleConfig): ModuleWithProviders<NgxAccountLibraryModule> {
    return {
      ngModule: NgxAccountLibraryModule,
      providers: [{ provide: ACCOUNT_MODULE_CONFIG_TOKEN, useValue: config }],
    }
 }
}
