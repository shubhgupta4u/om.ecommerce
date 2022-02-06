import { Injectable } from '@angular/core';
import { Router, NavigationStart } from '@angular/router';
import { Observable, Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AlertNotifierService {
  private alertMessages$ = new Subject<any>();
  private keepAfterNavigationChange = false;

  constructor(private router: Router) {
      // clear alert message on route change
      router.events.subscribe(event => {
          if (event instanceof NavigationStart) {
              if (this.keepAfterNavigationChange) {
                  // only keep for a single location change
                  this.keepAfterNavigationChange = false;
              } else {
                  // clear alert
                  this.alertMessages$.next();
              }
          }
      });
  }

  success(message: string, keepAfterNavigationChange = false) {
      this.keepAfterNavigationChange = keepAfterNavigationChange;
      this.alertMessages$.next({ type: 'success', text: message });
  }

  error(message: string, keepAfterNavigationChange = false) {
      this.keepAfterNavigationChange = keepAfterNavigationChange;
      this.alertMessages$.next({ type: 'error', text: message });
  }

  getMessage(): Observable<any> {
      return this.alertMessages$.asObservable();
  }
}
