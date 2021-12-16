import { NgModule, ModuleWithProviders, SkipSelf, Optional } from '@angular/core';
import { Configuration } from './configuration';
import { HttpClient } from '@angular/common/http';


import { ActorsService } from './api/actors.service';
import { BreakdownService } from './api/breakdown.service';
import { MilestonesService } from './api/milestones.service';
import { ReasonsService } from './api/reasons.service';
import { RepositoriesService } from './api/repositories.service';
import { RequirementSetsService } from './api/requirementSets.service';
import { RequirementsService } from './api/requirements.service';

@NgModule({
  imports:      [],
  declarations: [],
  exports:      [],
  providers: [
    ActorsService,
    BreakdownService,
    MilestonesService,
    ReasonsService,
    RepositoriesService,
    RequirementSetsService,
    RequirementsService ]
})
export class ApiModule {
    public static forRoot(configurationFactory: () => Configuration): ModuleWithProviders<ApiModule> {
        return {
            ngModule: ApiModule,
            providers: [ { provide: Configuration, useFactory: configurationFactory } ]
        };
    }

    constructor( @Optional() @SkipSelf() parentModule: ApiModule,
                 @Optional() http: HttpClient) {
        if (parentModule) {
            throw new Error('ApiModule is already loaded. Import in your base AppModule only.');
        }
        if (!http) {
            throw new Error('You need to import the HttpClientModule in your AppModule! \n' +
            'See also https://github.com/angular/angular/issues/20575');
        }
    }
}
