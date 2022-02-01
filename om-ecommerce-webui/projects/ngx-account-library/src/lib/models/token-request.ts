import { AuthProvider } from "../interfaces/ngx-account-module-config"

export class TokenRequest
{
    userName:string
    password:string
    grantType:AuthProvider
    bearerToken:string
    scope:string
}