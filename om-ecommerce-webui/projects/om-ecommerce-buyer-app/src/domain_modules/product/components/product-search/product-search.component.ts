import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { AccountFacadeService } from 'ngx-account-library';
import { SignalRNotificationData, SignalRService } from 'ngx-om-common-library';

@Component({
  selector: 'app-product-search',
  templateUrl: './product-search.component.html',
  styleUrls: ['./product-search.component.scss']
})
export class ProductSearchComponent implements OnInit {

  constructor(private http: HttpClient, private readonly signalrService:SignalRService,private accountService: AccountFacadeService) { }

  ngOnInit(): void {
    const headers = { 'content-type': 'application/json'}  
    this.http.get('https://localhost:44369/security/api/v1/test/adminsecure',{'headers':headers}).subscribe((result)=>{
      console.log(result);
    });
    let eventName=[];
    const accessToken = this.accountService.tokenResponseValue.jwtToken;
    eventName.push('OrderStatusChanged');
    this.signalrService.registerSignalRNotificationListener(accessToken,"order",eventName).subscribe((notificationData:SignalRNotificationData) => {
      console.log(notificationData);
    });

  }

}
