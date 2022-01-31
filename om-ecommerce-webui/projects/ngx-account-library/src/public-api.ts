/*
 * Public API Surface of ngx-account-library
 */

import { InjectionToken } from '@angular/core';
import { AccountModuleConfig } from './lib/interfaces/ngx-account-module-config';

export * from './lib/services/account-facade.service';
export * from './lib/services/auth-guard.service';
export * from './lib/services/jwt-interceptor.service';
export * from './lib/ngx-account-library.module';

export * from './lib/components/login/login.component';
export * from './lib/components/logout/logout.component';


export * from './lib/models/token-response'

export const ACCOUNT_MODULE_CONFIG_TOKEN = new InjectionToken<AccountModuleConfig>('Account_Module_Config');