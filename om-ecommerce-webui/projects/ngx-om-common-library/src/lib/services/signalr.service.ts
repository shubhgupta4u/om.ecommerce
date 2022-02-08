import { Injectable, Injector } from '@angular/core';
import { HttpTransportType, HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { CommonSetting } from '../interfaces/setting';
import { Common_Setting_TOKEN } from '../../public-api';
import { SignalRNotificationData } from '../models/signalr-notification-data';
import { LogLevel } from '@azure/msal-browser';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private signalrNotificationListener$ = new Subject<SignalRNotificationData>();
  private signalrUrl:string;
  private hubConnection: HubConnection

  public connectionId : string;

  constructor(private injector: Injector) { 
    const setting = injector.get<CommonSetting>(Common_Setting_TOKEN);
    if(setting){
      this.signalrUrl = setting.signalrUrl;
    }
    else{
      throw new Error("Failed to initialize SignalR Service. SignalR Url is required parameter.");
    }
  }

  public registerSignalRNotificationListener(accessToken:string,hubName:string, eventNames:string[]){
    this.startConnection(accessToken,hubName,eventNames)
      .then(()=>{
        
      },error=>{
        if(eventNames && eventNames.length > 0){
          this.signalrNotificationListener$.error(error);
        }
        console.log('Error while starting connection: ' + error.error)
      });
    return this.signalrNotificationListener$.asObservable(); 
  }

  private async startConnection(accessToken:string, hubName:string, eventNames:string[])
   {
    this.hubConnection = new HubConnectionBuilder()
                            .configureLogging("Trace")
                            .withUrl(`${this.signalrUrl}/${hubName}`, {
                              skipNegotiation: true,
                              transport: HttpTransportType.WebSockets,
                              accessTokenFactory: () => {
                                return accessToken;
                              }
                            })
                            .withAutomaticReconnect([0, 3000, 5000, 10000, 15000, 30000])
                            .build();
    await this.hubConnection
      .start()
      .then(() => {
          if(this.hubConnection.connectionId){
              this.connectionId =this.hubConnection.connectionId; 
              console.log('Connection started')
          }
      })
      .then(()=> this.addHubMethodListener(eventNames))
      .catch(err => {throw err});
  }
  private async addHubMethodListener(eventNames:string[]) {
    if(eventNames && eventNames.length > 0){
      eventNames.forEach(eventName => {
        this.hubConnection.on(eventName, (data) => {
          let signalrNotificationData: SignalRNotificationData = new SignalRNotificationData();
          signalrNotificationData.eventName = eventName;
          signalrNotificationData.data=data;
          this.signalrNotificationListener$.next(signalrNotificationData);
        });
      });
    }    
  }
}