/*
 * Public API Surface of ngx-om-common-library
 */

import { InjectionToken } from '@angular/core';
import { CommonSetting } from './lib/interfaces/setting';

export * from './lib/ngx-om-common-library.module';
export * from './lib/components/alert/alert.component';
export * from './lib/services/crypto.service';
export * from './lib/services/alert-notifier.service';
export * from './lib/services/signalr.service';
export * from './lib/interfaces/setting'
export * from './lib/pipes/safe-html.pipe'
export * from './lib/models/signalr-notification-data'
export * from './lib/models/order-status'

export const Common_Setting_TOKEN = new InjectionToken<CommonSetting>('Common_Setting');