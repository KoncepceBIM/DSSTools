import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'bd-popup',
  template: `
    <span ngDropdown [renderInPlace]="false" #infoDropdown="ngDropdown" *ngIf="node">
      <i class="fas fa-info-circle" ngDropdownControl></i>
      <div ngDropdownContent dropdownClass="slide-fade infodropbox" >
          <table>
              <tr><th colspan=2><h3>{{ node.nameCS }}</h3></th></tr>
              <tr>
                  <th>Popis:</th>
                  <td>{{ node.descriptionCS }}</td>
              </tr>
              <tr>
                  <th>Poznámka: </th>
                  <td>{{ node.noteCS }}</td>

              </tr>
              <tr>
                  <td colspan=2><br></td>
              </tr>
              <tr>
                  <th>IFC typ:</th>
                  <td>{{ node.ifcPredefinedType}}</td>
              </tr>
              <tr>
                  <th>IFC třída:</th>
                  <td>{{ node.ifcType}}</td>
              </tr>
              <tr>
                  <th>GUID</th>
                  <td>{{ node.uuid }}</td>
              </tr>
          </table>
              <!-- <div *ngFor="let item of node | keyvalue" >{{item.key}} : {{item.value}}</div> -->
        </div>
    </span>


  `,
  styleUrls: ['./popup.component.scss']
})
export class BreakdownRequirementsPopupComponent implements OnInit {

  @Input() node: any;
  constructor() { }

  ngOnInit(): void {
  }

}
