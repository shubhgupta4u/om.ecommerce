export interface AccountModuleConfig{
    readonly baseSecuirtyApiUrl:string
    readonly loginApiEndpoint:string
    readonly logoutApiEndpoint:string
    readonly refreshApiEndpoint:string
    readonly loginSuccessRoutePath:string;
    readonly logoutSuccessRoutePath:string;
}