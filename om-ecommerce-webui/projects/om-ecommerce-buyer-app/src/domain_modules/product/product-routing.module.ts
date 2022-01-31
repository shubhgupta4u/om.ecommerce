import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ProductSearchComponent } from './components/product-search/product-search.component';

const routes: Routes = [
  { path: '', component: ProductSearchComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ProductRoutingModule { }
