import { Component, OnInit } from '@angular/core';
import { AlertNotifierService } from '../../services/alert-notifier.service';

@Component({
  selector: 'lib-alert',
  templateUrl: './alert.component.html',
  styleUrls: ['./alert.component.css']
})
export class AlertComponent implements OnInit {

  message: any;
  constructor(private alertService: AlertNotifierService) { }

  ngOnInit() {
    this.alertService.getMessage().subscribe(message => {
      this.message = message;
    });
  }
  close(event:any){
    event.preventDefault();
    this.message=null;
  }
}
