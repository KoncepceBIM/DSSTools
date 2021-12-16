import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

import { TreeModule } from '@circlon/angular-tree-component';
import { AngularDropdownModule } from 'angular-dropdown';



import { AppComponent } from './app.component';
import { ApiModule } from './swagger/api.module';

import { Configuration } from './swagger/configuration';
import { FilterBoxComponent } from './filter-box/filter-box.component';
import { BreakdownRequirementsComponent } from './breakdown-requirements/breakdown-requirements.component';
import { BreakdownRequirementsPopupComponent } from './breakdown-requirements/popup.component';

function getConfig() {
  return new Configuration({
      //basePath: "https://localhost:5001", 
      basePath: ' ', // mezera! bez ni to nejak zprasi a nelogicky posila na 0.0.0.0/
  });
}



@NgModule({
  declarations: [
    AppComponent,
    FilterBoxComponent,
    BreakdownRequirementsComponent,
    BreakdownRequirementsPopupComponent,
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    TreeModule,
    AngularDropdownModule,
    ApiModule,//.forRoot({}),
    FormsModule,
  ],
  providers: [

    {
      provide: Configuration,
      useFactory: getConfig,
      //deps: MyAuthService,
      multi: false
  }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
