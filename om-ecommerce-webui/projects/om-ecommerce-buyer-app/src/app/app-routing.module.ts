import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from 'ngx-account-library';

const accountModule = () => import('ngx-account-library').then(x => x.NgxAccountLibraryModule);
const productModule = () => import('../domain_modules/product/product.module').then(x => x.ProductModule);

const routes: Routes = [
    // { path: 'login', component: LoginComponent },
    { path: 'account', loadChildren: accountModule },
    { path: 'product', loadChildren: productModule , canActivate: [AuthGuard] },
    // otherwise redirect to home
    { path: '**', redirectTo: '' }
];
@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
