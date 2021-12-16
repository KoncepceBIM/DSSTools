import { Component, OnInit, Input } from '@angular/core';
//import { Requirement } from '../swagger/model/requirement';
import { Requirement, RequirementSet, GrouppedRequirements, GrouppedRequirementSets, BreakdownItem } from '../swagger/model/models';

@Component({
  selector: 'breakdown-requirements',
  templateUrl: './breakdown-requirements.component.html',
  styleUrls: ['./breakdown-requirements.component.scss']
})
export class BreakdownRequirementsComponent implements OnInit {

  //@Input('data') requirements: Requirement[];
  @Input('data') data: GrouppedRequirements[]|GrouppedRequirementSets[];
  @Input() tree: any[];
  @Input() flat: any[];
  @Input() ifc: boolean;
  @Input() style: string;

  public cols: number = 11;
  public indent: number = 15;

  public hidePath = true;
  public collapsed = {};
  public maxlevel = 0;

  constructor() { }

  ngOnInit(): void {
    //console.log(this.data);
  }

  ngOnChanges() {
    //console.log(this.flat);
    //console.log(this.tree);
    this.cols = (this.style=='req'?8:7)+(this.ifc?4:0);
    this.maxlevel = 0;
    this.flat.forEach( f => { if (f.level > this.maxlevel) this.maxlevel=f.level });
  }

/*   reqCollapse(value) {
    this.flat.forEach( (node)=> this.collapsed[node.id] = value );
  } */


  findNodeInTreeById(tree, id) {
    const stack = []
    stack.push(...tree);
    while (stack.length) {
      const node = stack.shift();
      if (node.id === id) return node
      node.children && stack.push(...node.children)
    }
    return null
  }

  collapseRoot(value) {
    this.tree.forEach( ch => { this.collapsed[ch.id] = value; });
  }

  //sbal vsechny childy uzlu konkretniho ID
  collapsePath(id, value) {
    let node = this.findNodeInTreeById(this.tree, id);
    //console.log(node);
    if (!node) console.error('Internal Error: node not found - id=',id);
    node && node.children && node.children.forEach( ch => { this.collapsed[ch.id] = value; });
  }

  // vrati jen cast pole ktera je rozbalena, kdyz narazi na prvni zabalenou polozku tak konec. Pro snazsi cyklus v html template
  collapseFilter(path: BreakdownItem[]) {
    let result: BreakdownItem[] = [];
    for (let i=0; i<path.length; i++) {
      result.push(path[i]);
      if (this.collapsed[path[i].id]) break;
    }
    return result;
  }

  // sbali strom od dane urovne
  public collapseByPathLevel(level: number) {
    this.collapsed={};
    this.flat.forEach( ch => { 
      // sbali pozadovanej level, kterej mozna neexistuje, proto ten otaznik
      //this.collapsed[ ch.path[level]?.id ] = true; 
      // sbali pozadovanej level, nebo posledni existujici .. coz ale nefunguje dle ocekavani kdyz nejsou DŠ jen na konci ale i u parent uzlu (IFC vetev)
      //this.collapsed[ ch.path[level>(ch.path.length-1)?ch.path.length-1:level].id ] = true; 
      //
      if (level>(ch.path.length-1)) {
        this.collapsed[ ch.id ] = true;
      } else {
        this.collapsed[ ch.path[level].id ] = true; 
      }
    });
  }
  // sbali strom az na uroven datovych sablon
  public collapseByPathLast() {
    this.collapsed={};
    this.flat.forEach( ch => { this.collapsed[ ch.id ] = true; });
  }

}
