import { Component, OnInit, ViewChild } from '@angular/core';
import { Observable } from 'rxjs';

import { 
  ActorsService, BreakdownService, MilestonesService, ReasonsService, RepositoriesService, RequirementsService, RequirementSetsService,
} from './swagger/api/api';

import { GroupingType, Requirement, RequirementSet, GrouppedRequirements, GrouppedRequirementSets } from './swagger/model/models';
import { TreeModel, TreeNode, TreeComponent, ITreeOptions } from '@circlon/angular-tree-component';
import { FilterBoxComponent } from "./filter-box/filter-box.component";
import { ExportExcelService } from "./export-excel.service";

export class Repolist {
  key: string;
  value: string;
}

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {

  @ViewChild('breaks') fbBreaks: FilterBoxComponent;
  @ViewChild('milestones') fbMilestones: FilterBoxComponent;
  @ViewChild('reasons') fbReasons: FilterBoxComponent;
  @ViewChild('actors') fbActors: FilterBoxComponent;
  //@ViewChild('export_link') export_link: any;
  //@ViewChild('repo') fbRepo: FilterBoxComponent;
  //@ViewChild('repo') repo: nativeElement;
  public errors:string[] = [];
  public repo: string = "latest";
  public repoValue: string = "aktuální";
  //public help_url: string = 'https://www.koncepcebim.cz/dokumenty?dok=1063-napoveda-dss-online';
  public help_url: string = 'https://www.koncepcebim.cz/uploads/inq/files/DSS_online_pruvodce_Agentura%20CAS%20%281%29.pdf';
  public sel: string[];
/*   options = {
    useCheckbox: true
  }; */
  public ifc: boolean = false;

  public initdone: boolean = false;

  public nodesBreakdown: any[];
  public nodesActors: any[];
  public nodesMilestones: any[];
  public nodesReasons: any[];
  //public nodesRepositories: any[];
  public repolist: Repolist[];

  public selectedTree: any[] = null;
  public selectedFlatTree: any[] = null;
  public dataLoading: boolean = false;
  public dataTemplates: any[] = [];
  public reqtab: string = 'sets_cz';

  constructor(
    private actorsService: ActorsService,
    private breakdownService: BreakdownService,
    private milestonesService: MilestonesService,
    private reasonsService: ReasonsService,
    private repositoriesService: RepositoriesService,
    private requirementsService: RequirementsService,
    private requirementsSetsService: RequirementSetsService,
    private exportexcel: ExportExcelService,
  ) { }

  ngOnInit(): void {
    this.repositoriesService.apiRepositoriesGet().subscribe(
      d => {  
        this.repolist = [ { key:'latest', value: 'aktuální' } ];
        //this.repolist.push(...d.sort().reverse());
        d.sort().reverse().forEach( r => { this.repolist.push({ key: r, value: r }); });
      },
      e => { this.apiError(e); }
    );
    this.fetchFilters();
    this.initdone = true;
  }

  repoChange() {
    this.deselectAll();
    this.fetchFilters();
  }
  
  fetchFilters() {
    this.nodesActors = [];
    this.actorsService.apiRepositoryIdActorsGet(this.repo).subscribe({
      next: (d) => { this.nodesActors = d; },
      error: (e) => { this.apiError(e) },
    });
    this.nodesBreakdown = [];
    this.breakdownService.apiRepositoryIdBreakdownGet(this.repo, false).subscribe({
      next: (d) => {  this.nodesBreakdown = d; },
      error: (e) => { this.apiError(e) },
    });
    this.nodesMilestones = [];
    this.milestonesService.apiRepositoryIdMilestonesGet(this.repo).subscribe({
      next: (d) => {  this.nodesMilestones = d; },
      error: (e) => { this.apiError(e) },
    });
    this.nodesReasons = [];
    this.reasonsService.apiRepositoryIdReasonsGet(this.repo).subscribe({
      next: (d) => { this.nodesReasons = d; },
      error: (e) => { this.apiError(e) },
    });
  }

  runfilter() {

    this.dataLoading = true;
    this.errors = [];
    this.selectedTree = this.fbBreaks.getSelectedNodeTree();
    this.selectedFlatTree = this.fbBreaks.getSelectedIdsWithInfo();
    //console.log(this.selectedTree, this.selectedFlatTree);


    if (this.reqtab == 'req') {
      this.breakdownService.apiRepositoryIdBreakdownRequirementsGet(
        this.repo, 
        null, //select
        null, //filter
        null, //orderby
        null, //skip
        null, //top
        null, // apply
        this.fbActors.getSelectedIds().join(','), //actors
        this.fbReasons.getSelectedIds().join(','), //reasons
        this.fbBreaks.getSelectedIds().join(','), //breakdowns
        this.fbMilestones.getSelectedIds().join(','), //milestones
      ).subscribe({
        next: (r) => {
          //console.log(r); 
          //this.dataTemplates = r;
          this.dataTemplates = [];
          r.forEach( (req) => {
            this.dataTemplates[req.id] = req;
          });
          this.dataLoading = false;
          //console.log(this.dataTemplates);
        },
        error: (e) => { this.apiError(e) },
      });
    } else {
      let groupingtype:GroupingType = 'CS';
      switch (this.reqtab) {
        case "sets_cz": groupingtype='CS'; break;
        case "sets_en": groupingtype='EN'; break;
        case "sets_ifc": groupingtype='IFC'; break;
      }


      this.fetchSets(groupingtype).subscribe({
        next: (r) => {
          this.dataTemplates = [];
          r.forEach( (req) => {
            // pokud se jedna o skupinu co ma v nazvu obecná, tak ji chceme ve vychozim stavu sbalenou
            req.requirementSets.forEach( set => { if ( set.name.includes('obecn') ) (set as any).collapsed = true; });
            this.dataTemplates[req.id] = req;
            this.dataLoading = false;
          });
        },
        error: (e) => { this.apiError(e) },
      }
      );
    }

  } // end of runfilter

  public apiError(err) {
    //console.error("CHYBA", err);
    this.errors.push('Api request failed: '+err.status+' '+err.statusText);
    this.selectedTree = null;
    this.selectedFlatTree = null;
    this.dataLoading = false;
  }


  reqtabChange(tabname:string) {
    this.reqtab = tabname;
    this.runfilter();
  }

  deselectAll() {
    this.fbBreaks.deselectAll();
    this.fbMilestones.deselectAll();
    this.fbReasons.deselectAll();
    this.fbActors.deselectAll();
    this.dataTemplates = [];
    this.selectedTree = null;
    this.selectedFlatTree = null;
    this.errors = [];
  }


  exportIFC() {
    let url:string = '/api/'+this.repo+'/requirements/export?'+
      'actors='+this.fbActors?.getSelectedIds().join(',')+
      '&reasons='+this.fbReasons?.getSelectedIds().join(',')+
      '&breakdown='+this.fbBreaks?.getSelectedIds().join(',')+
      '&milestones='+this.fbMilestones?.getSelectedIds().join(',');

    let anchor = document.createElement("a");
    anchor.href = url;
    anchor.target = '_blank';
    anchor.click();
    anchor.remove();


/*     this.export_link.href = url;
    console.log(this.export_link);
    this.export_link.nativeElement.click(); // nefunguje
*/

  }

  public exportXLS() {
    this.dataLoading = true;
  
    let groupingtype:GroupingType = 'CS';

    this.fetchSets(groupingtype).subscribe({
      next: (r) => {
        //console.log(r); 
        var data = [];
        r.forEach( (req) => {
          data[req.id] = req;
        });
        this.exportexcel.export(data, this.fbBreaks.getSelectedNodeTree() );
        this.dataLoading = false;
      },
      error: (e) => { this.apiError(e) },
     }
    );
  

  }


  public fetchSets(groupingtype:GroupingType): Observable<any>  {
    return this.breakdownService.apiRepositoryIdBreakdownRequirementSetsGet(
      this.repo, 
      groupingtype, //grouping type
      null, //select
      null, //filter
      null, //orderby
      null, //skip
      null, //top
      null, // apply
      this.fbActors.getSelectedIds().join(','), //actors
      this.fbReasons.getSelectedIds().join(','), //reasons
      this.fbBreaks.getSelectedIds().join(','), //breakdowns
      this.fbMilestones.getSelectedIds().join(','), //milestones
    );
  }

}



