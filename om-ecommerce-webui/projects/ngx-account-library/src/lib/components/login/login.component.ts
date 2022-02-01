import { Component, Inject, OnInit, Optional } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { first } from 'rxjs/operators';
import { AccountFacadeService } from '../../services/account-facade.service';
import { AccountModuleConfig, AuthProvider } from '../../interfaces/ngx-account-module-config';
import { ACCOUNT_MODULE_CONFIG_TOKEN } from '../../../public-api';

@Component({
  selector: 'lib-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {

  form: FormGroup;
  loading = false;
  submitted = false;
  showForm=true;
  private _accountModuleConfig: AccountModuleConfig;

  constructor(
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    @Optional() @Inject(ACCOUNT_MODULE_CONFIG_TOKEN)
    private readonly config: AccountModuleConfig | null,
    private accountService: AccountFacadeService
  ) {
    if(this.router.url.indexOf("login/callback") >= 0){
      this.loading = true;
      this.accountService.handleOktaAuthentication().then((isAuthenticated)=>{
        this.loading = false;
      },error=>{
        this.loading = false;
      }
      );
    }
    if (config != null) {
      this._accountModuleConfig = config;
    }
    if(this._accountModuleConfig && this._accountModuleConfig.authProvider == AuthProvider.Okta){
      this.showForm=false;
    }
   
  }

  async ngOnInit() {
    if(this._accountModuleConfig && this._accountModuleConfig.authProvider == AuthProvider.NativeWebForm){
      this.form = this.formBuilder.group({
        username: ['', Validators.required],
        password: ['', Validators.required]
      });
    }
  }

  // convenience getter for easy access to form fields
  get f() { return this.form.controls; }

  onSubmit() {
    if(this._accountModuleConfig && this._accountModuleConfig.authProvider == AuthProvider.NativeWebForm){
      this.loginNativeWebForm();
    }
    else  if(this._accountModuleConfig && this._accountModuleConfig.authProvider == AuthProvider.Okta){
      this.loginOkta();
    }
  }
  
  loginNativeWebForm(){
    this.submitted = true;

    // stop here if form is invalid
    if (this.form.invalid) {
      return;
    }
    const returnUrl = this.route.snapshot.queryParams['returnUrl'];
    this.loading = true;
    this.accountService.webFormlogin(this.f.username.value, this.f.password.value,returnUrl)
      .pipe(first())
      .subscribe({
        next: () => {
          console.log("login successfully")
        },
        error: error => {
          this.loading = false;
          console.error("Error occured while logout")
          console.error(error);
        }
      });
  }

  loginOkta() {
    const returnUrl = this.route.snapshot.queryParams['returnUrl'];
    this.accountService.oktaLogin(returnUrl);
  }
}
