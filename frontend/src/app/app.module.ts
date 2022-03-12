import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { AppRoutingModule } from './app-routing.module';
import { HttpClientModule } from '@angular/common/http';

import { AppComponent } from './app.component';
import { LoginPageComponent } from './_pages/login-page/login-page.component';
import { RegisterPageComponent } from './_pages/register-page/register-page.component';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { OnlyAuthorizedComponent } from './_pages/only-authorized/only-authorized.component';
import { DatasetLoadComponent } from './_elements/dataset-load/dataset-load.component';
import { NgxCsvParser } from 'ngx-csv-parser';
import { AddModelComponent } from './_pages/add-model/add-model.component';

@NgModule({
  declarations: [
    AppComponent,
    LoginPageComponent,
    RegisterPageComponent,
    OnlyAuthorizedComponent,
    DatasetLoadComponent,
    AddModelComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    HttpClientModule,
    NgbModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }