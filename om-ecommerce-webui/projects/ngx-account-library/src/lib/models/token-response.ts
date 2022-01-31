export class TokenResponse
{
    jwtToken:string
    refreshToken:string
    userId:string
    name:string
    email:string
    mobile:string
    expiry:Date
    role:string[]
}