import { Inject, Injectable, Injector, Optional } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ACCOUNT_MODULE_CONFIG_TOKEN } from '../../public-api';
import { AccountModuleConfig, AuthProvider } from '../interfaces/ngx-account-module-config';
import { TokenRequest } from '../models/token-request';
import { TokenResponse } from '../models/token-response';
import { LoginService } from './login.service';
import { LogoutService } from './logout.service';
import jwt_decode from "jwt-decode";
import { OktaAuthService } from './okta-auth.service';
import { MsalAuthService } from './msal-auth.service';
import { CryptoService } from 'ngx-om-common-library';

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
  private _oktaAuthService: OktaAuthService;
  private _msalAuthService: MsalAuthService;

  private _accountModuleConfig: AccountModuleConfig;
  private refreshCallTimeout: any;

  public get msalAuthService(): MsalAuthService {
    if (!this._msalAuthService) {
      this._msalAuthService = this.injector.get(MsalAuthService);
    }
    return this._msalAuthService;
  }

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

  public get oktaAuthService(): OktaAuthService {
    if (!this._oktaAuthService) {
      this._oktaAuthService = this.injector.get(OktaAuthService);
    }
    return this._oktaAuthService;
  }

  constructor(private injector: Injector, private cryptoService:CryptoService,
    private router: Router,
    @Optional() @Inject(ACCOUNT_MODULE_CONFIG_TOKEN)
    private readonly config: AccountModuleConfig | null
  ) {

    if (config != null) {
      this._accountModuleConfig = config;
      var authstartedItem = sessionStorage.getItem("authstarted");
      if (authstartedItem && authstartedItem == "Msal") {        
        if (this.msalAuthService.isAuthenticated()) {
          this.handleMsalAuthentication();  
        } else {
          this.msalAuthService.authenticationResult$.subscribe((authResult) => {
            if (authResult && authResult.accessToken) {
              this.handleMsalAuthentication();
            }
          })
        }
      }
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

  webFormlogin(username: string, password: string, returnUrl: string) {

    var request = new TokenRequest();
    request.userName = username;
    request.password = this.cryptoService.encrypt(password);
    request.grantType = AuthProvider.NativeWebForm;
    return this.login(request, returnUrl);
  }

  login(request: TokenRequest, returnUrl: string | null) {
    request.scope = this._accountModuleConfig.nativeAuthApiConfig.authScope;
    this.clearRefreshTokenCall();
    localStorage.removeItem(this.TOKEN_STORAGE_KEY);
    this.tokenResponseSubject.next(this.defaultTokenResponse);

    return this.loginService.login(this._accountModuleConfig.nativeAuthApiConfig.loginUri, request)
      .pipe(map(response => {
        this.updateTokenWithJwtPayload(response);
        // store user details and jwt token in local storage to keep user logged in between page refreshes
        localStorage.setItem(this.TOKEN_STORAGE_KEY, JSON.stringify(response));
        this.tokenResponseSubject.next(response);
        this.registerRefreshTokenCall();
        const newReturnUrl = returnUrl || this._accountModuleConfig.nativeAuthApiConfig.loginRedirectRoute || '/';
        this.router.navigateByUrl(newReturnUrl);
      }));
  }
  msalLogin() {
    this.msalAuthService.login();
  }
  oktaLogin(returnUrl: string) {
    this.oktaAuthService.login(returnUrl);
  }

  handleOktaAuthentication() {
    return new Promise<boolean>((resolve, reject) => {
      this.oktaAuthService.handleAuthentication().then((token) => {
        if (token) {
          var request = new TokenRequest();
          request.bearerToken = token;
          request.grantType = AuthProvider.Okta;
          this.login(request, null).subscribe(() => {
            resolve(true);
          }, err => {
            reject(false);
          });
        } else {
          reject(false);
        }
      }, (error) => {
        reject(error);
      });
    })

  }
  handleMsalAuthentication() {
    sessionStorage.removeItem("authstarted");
    return new Promise<boolean>((resolve, reject) => {
      var accessToken = this.msalAuthService.getAccessToken();
      if (accessToken) {
        var request = new TokenRequest();
        request.bearerToken = accessToken;
        request.grantType = AuthProvider.AzureAD;
        this.login(request, null).subscribe(() => {
          resolve(true)
        }, err => {
          reject(false);
        });
      } else {
        reject(false);
      }
    });
  }
  msalLogout() {
    this.msalAuthService.logout();
  }
  oktaLogout() {
    this.oktaAuthService.logout();
  }

  logout(returnUrl: string) {
    return this.logoutService.logout(this._accountModuleConfig.nativeAuthApiConfig.logoutUri)
      .pipe(map(response => {
        this.clearRefreshTokenCall();
        // remove user from local storage and set current user to null
        localStorage.removeItem(this.TOKEN_STORAGE_KEY);
        this.tokenResponseSubject.next(this.defaultTokenResponse);
        const newReturnUrl = returnUrl || this._accountModuleConfig.nativeAuthApiConfig.logoutRedirectRoute || '/';
        this.router.navigateByUrl(newReturnUrl);
      }));
  }

  refresh(tokenRespone: TokenResponse) {
    this.clearRefreshTokenCall();

    return this.loginService.refresh(this._accountModuleConfig.nativeAuthApiConfig.refreshUri, this.tokenResponseValue)
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
    if (tokenRespone) {
      var diff = Math.abs((new Date()).getTime() - tokenRespone.expiry.getTime());
      var diffInMin = Math.ceil(diff / (1000 * 60));
      var timeoutMilliSec = (diffInMin - this.REFRESH_TOKEN_DELAY) * 60 * 1000;
      this.refreshCallTimeout = setTimeout(() => {
        var tokenRespone = this.tokenResponseValue;
        if (tokenRespone && tokenRespone.refreshToken && tokenRespone.jwtToken) {
          this.refresh(tokenRespone).subscribe(() => {
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
