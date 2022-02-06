import { ModuleWithProviders, NgModule } from '@angular/core';
import { AlertComponent } from './components/alert/alert.component';
import { CommonSetting } from './interfaces/setting';
import { Common_Setting_TOKEN } from '../public-api';
import { SafeHtmlPipe } from './pipes/safe-html.pipe';
import { CommonModule } from '@angular/common';


@NgModule({
  declarations: [
    AlertComponent,
    SafeHtmlPipe
  ],
  imports: [
    CommonModule
  ],
  exports: [
    AlertComponent,
    SafeHtmlPipe
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
