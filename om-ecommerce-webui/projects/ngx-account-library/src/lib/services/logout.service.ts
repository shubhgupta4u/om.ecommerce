import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class LogoutService {

  constructor(private http: HttpClient) { }

  logout(baseUri: string, endpoint: string) {
    const headers = { 'content-type': 'application/json'}  
    const body=JSON.stringify({});
    return this.http.post(`${baseUri}/${endpoint}`, body,{'headers':headers}); 
  }
}
