import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AccountFacadeService } from '../../services/account-facade.service';

@Component({
  selector: 'lib-logout',
  templateUrl: './logout.component.html',
  styleUrls: ['./logout.component.css']
})
export class LogoutComponent implements OnInit {

  constructor(private accountService: AccountFacadeService,private route: ActivatedRoute) { }

  ngOnInit(): void {
    const returnUrl = this.route.snapshot.queryParams['returnUrl'];
    this.accountService.logout(returnUrl).subscribe(()=>{
      console.log("logout successfully")
    },error=>{
      console.error("Error occured while logout")
      console.error(error);
    })
  }

}
