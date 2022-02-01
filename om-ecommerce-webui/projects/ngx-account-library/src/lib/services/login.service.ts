import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { TokenResponse } from '../models/token-response';
import { TokenRequest } from '../models/token-request';

@Injectable({
  providedIn: 'root'
})
export class LoginService {

  constructor(private http: HttpClient) {    
  }
 
  login(uri: string, tokenRequest: TokenRequest) {
    const headers = { 'content-type': 'application/json'}  
    const body=JSON.stringify(tokenRequest);
    return this.http.post<TokenResponse>(uri, body,{'headers':headers})
      .pipe(map(response => {
        return response;
      }));
  } 
  refresh(uri: string, tokenResponse: TokenResponse) {
    const headers = { 'content-type': 'application/json'}  
    const body=JSON.stringify({"jwtToken":tokenResponse.jwtToken,"refreshToken":tokenResponse.refreshToken});
    return this.http.post<TokenResponse>(uri, body,{'headers':headers})
      .pipe(map(response => {
        return response;
      }));
  } 
}
