import { Component, Inject, OnDestroy, OnInit, Optional } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { first } from 'rxjs/operators';
import { AccountFacadeService } from '../../services/account-facade.service';
import { AccountModuleConfig, AuthProvider } from '../../interfaces/ngx-account-module-config';
import { ACCOUNT_MODULE_CONFIG_TOKEN } from '../../../public-api';
import { MsalService } from '@azure/msal-angular';
import { AlertNotifierService } from 'ngx-om-common-library';

@Component({
  selector: 'lib-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {

  form: FormGroup;
  loading = false;
  submitted = false;
  showForm = true;
  private _accountModuleConfig: AccountModuleConfig;

  constructor(
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private authService: MsalService,
    private alertService: AlertNotifierService,
    @Optional() @Inject(ACCOUNT_MODULE_CONFIG_TOKEN)
    private readonly config: AccountModuleConfig | null,
    private accountService: AccountFacadeService
  ) {
    var authstartedItem = sessionStorage.getItem("authstarted");
    if (authstartedItem && authstartedItem == "Okta") {
      this.loading = true;
      sessionStorage.removeItem("authstarted");
      this.accountService.handleOktaAuthentication().then((isAuthenticated) => {
        console.log("Successfully login using Okta credential")
      }, error => {
        this.loading = false;
      }
      );
    }

    if (config != null) {
      this._accountModuleConfig = config;
    }
    if (this._accountModuleConfig && this._accountModuleConfig.authProvider != AuthProvider.NativeWebForm) {
      this.showForm = false;
    }
  }

  async ngOnInit() {
    if (this._accountModuleConfig && this._accountModuleConfig.authProvider == AuthProvider.NativeWebForm) {
      this.form = this.formBuilder.group({
        username: [null, Validators.required],
        password: [null, Validators.required]
      });
    }
  }

  // convenience getter for easy access to form fields
  get f() { return this.form.controls; }

  onSubmit(authProvider:number) {
    this.loading = true;
    if (this._accountModuleConfig && authProvider == AuthProvider.NativeWebForm) {
      this.loginNativeWebForm();
    }
    else if (this._accountModuleConfig && authProvider == AuthProvider.Okta) {
      this.loginOkta();
    }
    else if (this._accountModuleConfig && authProvider == AuthProvider.AzureAD) {
      this.loginMsal();
    }
  }

  loginNativeWebForm() {
    this.submitted = true;
    sessionStorage.removeItem("authstarted");
    // stop here if form is invalid
    if (this.form.invalid) {
      this.loading = false;
      return;
    }
    const returnUrl = this.route.snapshot.queryParams['returnUrl'];
    this.accountService.webFormlogin(this.f.username.value, this.f.password.value, returnUrl)
      .pipe(first())
      .subscribe({
        next: () => {
          console.log("login successfully")
        },
        error: error => {
          this.loading = false;
          console.error(error);
          this.alertService.error(error.error);
        }
      });
  }

  loginOkta() {
    const returnUrl = this.route.snapshot.queryParams['returnUrl'];
    this.accountService.oktaLogin(returnUrl);
    sessionStorage.setItem("authstarted","Okta")
  }

  loginMsal() {
    this.accountService.msalLogin();
    sessionStorage.setItem("authstarted","Msal")
  }
}
