import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { first } from 'rxjs/operators';
import { AccountFacadeService } from '../../services/account-facade.service';

// import { AlertService } from '@app/_services';

@Component({
  selector: 'lib-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {

  form: FormGroup;
  loading = false;
  submitted = false;

  constructor(
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private accountService: AccountFacadeService//,
    // private alertService: AlertService
  ) {
    
  }

  ngOnInit() {
    this.form = this.formBuilder.group({
      username: ['', Validators.required],
      password: ['', Validators.required]
    });
  }

  // convenience getter for easy access to form fields
  get f() { return this.form.controls; }

  onSubmit() {
    this.submitted = true;

    // reset alerts on submit
    // this.alertService.clear();

    // stop here if form is invalid
    if (this.form.invalid) {
      return;
    }
    const returnUrl = this.route.snapshot.queryParams['returnUrl'];
    this.loading = true;
    this.accountService.login(this.f.username.value, this.f.password.value,returnUrl)
      .pipe(first())
      .subscribe({
        next: () => {
          console.log("login successfully")
        },
        error: error => {
          // this.alertService.error(error);
          this.loading = false;
          console.error("Error occured while logout")
          console.error(error);
        }
      });
  }

}
