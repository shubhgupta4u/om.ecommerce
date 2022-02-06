/*
 * Public API Surface of ngx-om-common-library
 */

import { InjectionToken } from '@angular/core';
import { CommonSetting } from './lib/interfaces/setting';

export * from './lib/ngx-om-common-library.module';
export * from './lib/components/alert/alert.component';
export * from './lib/components/validation-message/validation-message.component';
export * from './lib/services/crypto.service';
export * from './lib/interfaces/setting'

export const Common_Setting_TOKEN = new InjectionToken<CommonSetting>('Common_Setting');