import { Inject, Injectable, Optional } from '@angular/core';
import { OktaAuth, IDToken, AccessToken } from '@okta/okta-auth-js';
import { AccountModuleConfig } from '../interfaces/ngx-account-module-config';
import { ACCOUNT_MODULE_CONFIG_TOKEN } from '../../public-api';

@Injectable({
  providedIn: 'root'
})
export class OktaAuthService {

  oktaAuth:OktaAuth;
  private _accountModuleConfig: AccountModuleConfig;

  constructor(
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
  }


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

    return tokenContainer.tokens.accessToken?.accessToken;
  }

  async logout() {
    await this.oktaAuth.signOut({
      postLogoutRedirectUri: this._accountModuleConfig.oktaAuthConfig.logoutRedirectUri
    });
  }
}
