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
import jwt_decode from "jwt-decode";

@Injectable({
  providedIn: 'root'
})
export class AccountFacadeService {

  readonly TOKEN_STORAGE_KEY = "TOKEN_STORAGE_KEY";
  readonly REFRESH_TOKEN_DELAY = 2; //min 

  private defaultTokenResponse: TokenResponse;
  private tokenResponseSubject: BehaviorSubject<TokenResponse>;
  public tokenResponse: Observable<TokenResponse>;

  private _loginService: LoginService;
  private _logoutService: LogoutService;
  private _accountModuleConfig: AccountModuleConfig;
  private refreshCallTimeout: any;

  public get loginService(): LoginService {
    if (!this._loginService) {
      this._loginService = this.injector.get(LoginService);
    }
    return this._loginService;
  }
  public get logoutService(): LogoutService {
    if (!this._logoutService) {
      this._logoutService = this.injector.get(LogoutService);
    }
    return this._logoutService;
  }
  constructor(private injector: Injector,
    private router: Router,
    @Optional() @Inject(ACCOUNT_MODULE_CONFIG_TOKEN)
    private readonly config: AccountModuleConfig | null
  ) {

    if (config != null) {
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

  login(username: string, password: string, returnUrl: string) {

    var request = new TokenRequest();
    request.userName = username;
    request.password = password;
    request.grantType = "password";
    request.scope = "om.ecommerce";

    this.clearRefreshTokenCall();
    localStorage.removeItem(this.TOKEN_STORAGE_KEY);
    this.tokenResponseSubject.next(this.defaultTokenResponse);

    return this.loginService.login(this._accountModuleConfig.baseSecuirtyApiUrl, this._accountModuleConfig.loginApiEndpoint, request)
      .pipe(map(response => {
        this.updateTokenWithJwtPayload(response);
        // store user details and jwt token in local storage to keep user logged in between page refreshes
        localStorage.setItem(this.TOKEN_STORAGE_KEY, JSON.stringify(response));
        this.tokenResponseSubject.next(response);
        this.registerRefreshTokenCall();
        const newReturnUrl = returnUrl || this._accountModuleConfig.loginSuccessRoutePath || '/';
        this.router.navigateByUrl(newReturnUrl);
      }));
  }

  logout(returnUrl: string) {
    return this.logoutService.logout(this._accountModuleConfig.baseSecuirtyApiUrl, this._accountModuleConfig.logoutApiEndpoint)
      .pipe(map(response => {
        this.clearRefreshTokenCall();
        // remove user from local storage and set current user to null
        localStorage.removeItem(this.TOKEN_STORAGE_KEY);
        this.tokenResponseSubject.next(this.defaultTokenResponse);
        const newReturnUrl = returnUrl || this._accountModuleConfig.logoutSuccessRoutePath || '/';
        this.router.navigateByUrl(newReturnUrl);
      }));
  }

  refresh(tokenRespone: TokenResponse) {
    this.clearRefreshTokenCall();

    return this.loginService.refresh(this._accountModuleConfig.baseSecuirtyApiUrl, this._accountModuleConfig.refreshApiEndpoint, this.tokenResponseValue)
      .pipe(map(response => {
        this.updateTokenWithJwtPayload(response);
        // store user details and jwt token in local storage to keep user logged in between page refreshes
        localStorage.setItem(this.TOKEN_STORAGE_KEY, JSON.stringify(response));
        this.tokenResponseSubject.next(response);       
      }));
  }

  private updateTokenWithJwtPayload(tokenRespone: TokenResponse) {
    if (tokenRespone) {
      var decodedJwt: any = jwt_decode(tokenRespone.jwtToken);
      if (decodedJwt) {
        tokenRespone.userId = decodedJwt['nameid'];
        tokenRespone.name = decodedJwt['unique_name'];
        tokenRespone.email = decodedJwt['email'];
        tokenRespone.mobile = decodedJwt['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/mobilephone'];
        tokenRespone.expiry = new Date(+decodedJwt['exp'] * 1000);
        tokenRespone.role = [];
        tokenRespone.role.push(decodedJwt['role']);
      }
    }
  }

  registerRefreshTokenCall() {
    var tokenRespone = this.tokenResponseValue;
    if(tokenRespone){
      var diff = Math.abs((new Date()).getTime() - tokenRespone.expiry.getTime());
      var diffInMin = Math.ceil(diff / (1000 * 60)); 
      var timeoutMilliSec = (diffInMin - this.REFRESH_TOKEN_DELAY) * 60 * 1000;
      this.refreshCallTimeout = setTimeout(() => {
        var tokenRespone = this.tokenResponseValue;
        if (tokenRespone && tokenRespone.refreshToken && tokenRespone.jwtToken) {
          this.refresh(tokenRespone).subscribe(()=>{
            this.registerRefreshTokenCall();
          });
        }
      }, timeoutMilliSec); //call refresh token 2 min before token expiry
    }   
  }

  clearRefreshTokenCall() {
    if (this.refreshCallTimeout) {
      clearTimeout(this.refreshCallTimeout);
    }
  }
}
