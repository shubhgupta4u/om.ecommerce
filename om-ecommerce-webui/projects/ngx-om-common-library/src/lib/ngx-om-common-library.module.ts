import { ModuleWithProviders, NgModule } from '@angular/core';
import { AlertComponent } from './components/alert/alert.component';
import { ValidationMessageComponent } from './components/validation-message/validation-message.component';
import { CommonSetting } from './interfaces/setting';
import { Common_Setting_TOKEN } from '../public-api';



@NgModule({
  declarations: [
    AlertComponent,
    ValidationMessageComponent
  ],
  imports: [
  ],
  exports: [
    AlertComponent,
    ValidationMessageComponent
  ]
})
export class NgxOmCommonLibraryModule {
  static forRoot(config: CommonSetting): ModuleWithProviders<NgxOmCommonLibraryModule> {
    return {
      ngModule: NgxOmCommonLibraryModule,
      providers: [{ provide: Common_Setting_TOKEN, useValue: config }],
    }
  }
 }
