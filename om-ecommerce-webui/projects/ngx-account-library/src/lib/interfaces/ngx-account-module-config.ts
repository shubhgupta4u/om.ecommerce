export const enum AuthProvider {
    NativeWebForm = 1,
    Okta=2,
    AzureAD=3,
  }

export interface AccountModuleConfig{
    readonly authProvider:AuthProvider;
    readonly oktaAuthConfig:OktaAuthConfig;
    readonly nativeAuthApiConfig:NativeAuthApiConfig
}
export interface NativeAuthApiConfig{
    readonly loginUri:string;
    readonly logoutUri:string;
    readonly refreshUri:string;
    readonly loginRedirectRoute:string;
    readonly logoutRedirectRoute:string;
    readonly authScope:string;
}
export interface OktaAuthConfig{
    readonly clientId:string;
    readonly issuer:string;
    readonly logoutRedirectUri:string;
    readonly loginRedirectUri:string;
}