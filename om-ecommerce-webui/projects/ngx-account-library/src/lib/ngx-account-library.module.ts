import { ModuleWithProviders, NgModule } from '@angular/core';
import { LoginComponent } from './components/login/login.component';
import { LogoutComponent } from './components/logout/logout.component';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { NgxAccountLibraryRoutingModule } from './ngx-account-library-routing.module';
import { AccountModuleConfig } from './interfaces/ngx-account-module-config';
import { ACCOUNT_MODULE_CONFIG_TOKEN} from '../public-api';
import { CommonModule } from '@angular/common';

@NgModule({
  declarations: [
    LoginComponent,
    LogoutComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    HttpClientModule,
    NgxAccountLibraryRoutingModule
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
