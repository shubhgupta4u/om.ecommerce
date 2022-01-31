import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AccountFacadeService } from './account-facade.service';

@Injectable({
  providedIn: 'root'
})
export class JwtInterceptor implements HttpInterceptor {
  constructor(private accountService: AccountFacadeService) { }

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
      // add auth header with jwt if user is logged in and request is to the api url
      const tokenResponse = this.accountService.tokenResponseValue;
      const isLoggedIn = tokenResponse && tokenResponse.jwtToken;
    
      if (isLoggedIn) {
          request = request.clone({
              setHeaders: {
                  Authorization: `Bearer ${tokenResponse.jwtToken}`
              }
          });
      }

      return next.handle(request);
  }
}
