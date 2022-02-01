import { Observable, Observer } from 'rxjs';
import { Inject, Injectable, Optional } from '@angular/core';
import { Router } from '@angular/router';
import { OktaAuth, IDToken, AccessToken } from '@okta/okta-auth-js';
import { AccountModuleConfig } from '../interfaces/ngx-account-module-config';
import { ACCOUNT_MODULE_CONFIG_TOKEN } from '../../public-api';

@Injectable({
  providedIn: 'root'
})
export class OktaAuthService {

  // IMPORTANT!
  // Replace ${clientId} with your actual Client ID
  // Replace ${yourOktaDomain} with your actual Okta domain
  // If using a custom authorization server, ISSUER should be 'https://${yourOktaDomain}/oauth2/${authorizationServerId}'

  oktaAuth:OktaAuth;
  private _accountModuleConfig: AccountModuleConfig;
  // $isAuthenticated: Observable<boolean>;
  // private observer?: Observer<boolean>;

  constructor(private router: Router,
    @Optional() @Inject(ACCOUNT_MODULE_CONFIG_TOKEN)
    private readonly config: AccountModuleConfig | null) {

    if (config != null) {
      this._accountModuleConfig = config;
      if(this._accountModuleConfig && this._accountModuleConfig.oktaAuthConfig){
        this.oktaAuth = new OktaAuth({
          clientId: this._accountModuleConfig.oktaAuthConfig.clientId,
          issuer: this._accountModuleConfig.oktaAuthConfig.issuer,
          redirectUri: this._accountModuleConfig.oktaAuthConfig.loginRedirectUri,
          pkce: true
        });
      }
    }
    // this.$isAuthenticated = new Observable((observer: Observer<boolean>) => {
    //   this.observer = observer;
    //   this.isAuthenticated().then(val => {
    //     observer.next(val);
    //   });
    // });
  }

  // async isAuthenticated() {
  //   // Checks if there is a current accessToken in the TokenManger.
  //   return !!(await this.oktaAuth.tokenManager.get('accessToken'));
  // }

  login(originalUrl: string) {
    // Launches the login redirect.
    this.oktaAuth.token.getWithRedirect({
      scopes: ['openid', 'email', 'profile']
    });
  }

  async handleAuthentication() {
    const tokenContainer = await this.oktaAuth.token.parseFromUrl();

    this.oktaAuth.tokenManager.add('idToken', tokenContainer.tokens.idToken as IDToken);
    this.oktaAuth.tokenManager.add('accessToken', tokenContainer.tokens.accessToken as AccessToken);

    // if (await this.isAuthenticated()) {
    //   this.observer?.next(true);
    // }
    return tokenContainer.tokens.accessToken?.accessToken;
  }

  async logout() {
    await this.oktaAuth.signOut({
      postLogoutRedirectUri: this._accountModuleConfig.oktaAuthConfig.logoutRedirectUri
    });
  }
}
