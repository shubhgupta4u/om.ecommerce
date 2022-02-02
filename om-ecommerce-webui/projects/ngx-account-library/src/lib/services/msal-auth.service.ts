import { Inject, Injectable, Optional } from '@angular/core';
import { MsalService } from '@azure/msal-angular';
import { AuthenticationResult, BrowserCacheLocation, IPublicClientApplication, LogLevel, PublicClientApplication } from '@azure/msal-browser';
import { BehaviorSubject, fromEvent, Observable, Subscription } from 'rxjs';
import { ACCOUNT_MODULE_CONFIG_TOKEN } from '../../public-api';
import { AccountModuleConfig, AzureAdAuthConfig } from '../interfaces/ngx-account-module-config';

const isIE = window.navigator.userAgent.indexOf("MSIE ") > -1 || window.navigator.userAgent.indexOf("Trident/") > -1;

@Injectable({
  providedIn: 'root'
})
export class MsalAuthService {

  private authenticationResultSubject$: BehaviorSubject<AuthenticationResult|null>;
  public authenticationResult$: Observable<AuthenticationResult|null>;

  private azureAdAuthConfig: AzureAdAuthConfig;
  // private loadObservable$: Observable<Event>
  // private loadSubscription$: Subscription

  constructor(
    private authService: MsalService,
    @Optional() @Inject(ACCOUNT_MODULE_CONFIG_TOKEN)
    private readonly config: AccountModuleConfig | null
  ) { 
    if (config != null && config.azureAdAuthConfig) {
      this.azureAdAuthConfig = config.azureAdAuthConfig;
      this.authService.instance = this.msalInstanceFactory(this.azureAdAuthConfig);      
    }    
    this.authenticationResultSubject$ = new BehaviorSubject<AuthenticationResult|null>(null);
    this.authenticationResult$ = this.authenticationResultSubject$.asObservable();   
    this.checkAndFetachAuthenticationResult().then((authResult)=>{
      if(authResult && authResult.accessToken){
        this.authenticationResultSubject$.next(authResult);
      }
    });

    // this.loadObservable$ = fromEvent(window, 'load')
    // this.loadSubscription$ = this.loadObservable$.subscribe( evt => {
    //   console.log('event: ', evt)
    // })
  }
  msalInstanceFactory(azureAdAuthConfig: AzureAdAuthConfig): IPublicClientApplication {
    return new PublicClientApplication({
      auth: {
        clientId: azureAdAuthConfig.clientId, 
        authority: `https://login.microsoftonline.com/${azureAdAuthConfig.tenantId}`, 
        redirectUri: azureAdAuthConfig.loginRedirectUri,
        postLogoutRedirectUri: azureAdAuthConfig.logoutRedirectUri
      },
      cache: {
        cacheLocation: BrowserCacheLocation.LocalStorage,
        storeAuthStateInCookie: isIE, // set to true for IE 11. Remove this line to use Angular Universal
      }
    });
  }
  isAuthenticated(){
    return this.authenticationResultSubject$.value != null && this.authenticationResultSubject$.value.accessToken != null;
  }
  getAccessToken(){
    if(this.isAuthenticated()){
      return this.authenticationResultSubject$.value?.accessToken;
    }
    return null;
  }
  async checkAndFetachAuthenticationResult(){
    let authResult:AuthenticationResult|null = await this.authService.instance.handleRedirectPromise();
    return authResult;    
  }
  login() {
    this.authService.loginRedirect({
      scopes: this.azureAdAuthConfig.scopes
    });
  }

  logout() {
    this.authService.logout();
  }
  destroy() {
    console.log('ngOnDestroy: MsalAuthService cleaning up...');
    this.authenticationResultSubject$.unsubscribe();
  }
}
