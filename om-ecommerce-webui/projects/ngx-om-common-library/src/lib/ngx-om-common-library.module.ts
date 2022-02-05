import { NgModule } from '@angular/core';
import { NgxOmCommonLibraryComponent } from './ngx-om-common-library.component';
import { AlertComponent } from './components/alert/alert.component';
import { ValidationMessageComponent } from './components/validation-message/validation-message.component';



@NgModule({
  declarations: [
    NgxOmCommonLibraryComponent,
    AlertComponent,
    ValidationMessageComponent
  ],
  imports: [
  ],
  exports: [
    NgxOmCommonLibraryComponent,
    AlertComponent,
    ValidationMessageComponent
  ]
})
export class NgxOmCommonLibraryModule { }
