import { Inject, Injectable, Injector, Optional } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ACCOUNT_MODULE_CONFIG_TOKEN, LogoutComponent } from '../../public-api';
import { AccountModuleConfig } from '../interfaces/ngx-account-module-config';
import { TokenRequest } from '../models/token-request';
import { TokenResponse } from '../models/token-response';
import { LoginService } from './login.service';
import { LogoutService } from './logout.service';

@Injectable({
  providedIn: 'root'
})
export class AccountFacadeService {

  readonly TOKEN_STORAGE_KEY ="TOKEN_STORAGE_KEY";
  private defaultTokenResponse: TokenResponse;
  private tokenResponseSubject: BehaviorSubject<TokenResponse>;
  public tokenResponse: Observable<TokenResponse>;

  private _loginService: LoginService;
  private _logoutService: LogoutService;
  private _accountModuleConfig: AccountModuleConfig;

  public get loginService(): LoginService {
    if(!this._loginService){
      this._loginService = this.injector.get(LoginService);
    }
    return this._loginService;
  }
  public get logoutService(): LogoutService {
    if(!this._logoutService){
      this._logoutService = this.injector.get(LogoutService);
    }
    return this._logoutService;
  }
  constructor(private injector: Injector,
              private router: Router,
              @Optional() @Inject(ACCOUNT_MODULE_CONFIG_TOKEN)
              private readonly config: AccountModuleConfig | null
              ) {  

    if(config != null){
      this._accountModuleConfig = config;
    }    
    var tokenResponse = localStorage.getItem(this.TOKEN_STORAGE_KEY)
    if (tokenResponse) {
      this.tokenResponseSubject = new BehaviorSubject<TokenResponse>(JSON.parse(tokenResponse));
    }
    else {
      this.tokenResponseSubject = new BehaviorSubject<TokenResponse>(this.defaultTokenResponse);
    }
    this.tokenResponse = this.tokenResponseSubject.asObservable();
  }

  public get tokenResponseValue(): TokenResponse {
    return this.tokenResponseSubject.value;
  }
  
  login(username:string, password:string,returnUrl:string) {
    var request = new TokenRequest();
    request.userName = username;
    request.password = password;
    request.grantType = "password";
    request.scope ="om.ecommerce";

    localStorage.removeItem(this.TOKEN_STORAGE_KEY);
    this.tokenResponseSubject.next(this.defaultTokenResponse);

    return this.loginService.login(this._accountModuleConfig.baseSecuirtyApiUrl,this._accountModuleConfig.loginApiEndpoint, request)
    .pipe(map(response => {
      // store user details and jwt token in local storage to keep user logged in between page refreshes
      localStorage.setItem(this.TOKEN_STORAGE_KEY, JSON.stringify(response));
      this.tokenResponseSubject.next(response);
      const newReturnUrl = returnUrl || this._accountModuleConfig.loginSuccessRoutePath || '/';
      this.router.navigateByUrl(newReturnUrl);
    }));
  }

  logout(returnUrl: string) {
    return this.logoutService.logout(this._accountModuleConfig.baseSecuirtyApiUrl, this._accountModuleConfig.logoutApiEndpoint)
      .pipe(map(response => {
        // remove user from local storage and set current user to null
        localStorage.removeItem(this.TOKEN_STORAGE_KEY);
        this.tokenResponseSubject.next(this.defaultTokenResponse);
        const newReturnUrl = returnUrl || this._accountModuleConfig.logoutSuccessRoutePath || '/';
        this.router.navigateByUrl(newReturnUrl);
      }));
  }
}
