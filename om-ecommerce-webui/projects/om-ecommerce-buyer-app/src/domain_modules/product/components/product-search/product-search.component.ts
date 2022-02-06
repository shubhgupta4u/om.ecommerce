import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-product-search',
  templateUrl: './product-search.component.html',
  styleUrls: ['./product-search.component.scss']
})
export class ProductSearchComponent implements OnInit {

  constructor(private http: HttpClient) { }

  ngOnInit(): void {
    const headers = { 'content-type': 'application/json'}  
    this.http.get('https://localhost:44369/security/api/v1/test/adminsecure',{'headers':headers}).subscribe((result)=>{
      console.log(result);
    }) 
  }

}
