import { Component, OnInit, Input, ViewChild, ComponentRef} from '@angular/core';
import { ITreeOptions, TreeNode, ITreeState, TreeComponent, IActionMapping, TREE_ACTIONS  } from '@circlon/angular-tree-component';
import { ITreeNode } from '@circlon/angular-tree-component/lib/defs/api';

const actionMapping: IActionMapping = {
  mouse: {
    dblClick: (tree, node, $event) => {
      if (node.hasChildren) {
        TREE_ACTIONS.TOGGLE_EXPANDED(tree, node, $event);
      } else {
        TREE_ACTIONS.TOGGLE_SELECTED(tree, node, $event);
      }
    },
    click: (tree, node, $event) => {
      if (node.hasChildren) {
        TREE_ACTIONS.TOGGLE_EXPANDED(tree, node, $event);
      } else {
        TREE_ACTIONS.TOGGLE_SELECTED(tree, node, $event);
      }
    },
  }
};

@Component({
  selector: 'app-filter-box',
  templateUrl: './filter-box.component.html',
  styleUrls: ['./filter-box.component.scss']
})
export class FilterBoxComponent implements OnInit {
  @Input() name: string;
  @Input() nodes: any[];
  @Input() nodesSplit: boolean;
  @ViewChild('tree') tree: TreeComponent;
  @ViewChild('filter') filterfield;
  public hasChildren: boolean = false;
  public nodes_bak: any[];
  public split_name: string;
  //@Input() options: ITreeOptions;
  public selected: TreeNode[] = [];
  public short: number = 4;
  public selectedShortToggle: boolean = true;
  //state: ITreeState;

  options: ITreeOptions = {
    useCheckbox: true,
    useTriState: true,
    actionMapping,
    displayField: 'nameCS',
    //idField: 'uuid',
    //useVirtualScroll: true,
    //nodeHeight: 22,
    //nodeHeight: (node: TreeNode) => node.height,
  };

  constructor() { }

  ngOnInit(): void {
  }
  
  ngOnChanges() {
    //console.log('change', this.name, this.nodesSplit, this.nodes);
    if (this.nodes) {
      if (this.nodesSplit && this.nodes[0]) {
        this.nodes_bak = this.nodes;
        this.nodes     = this.nodes_bak[0]?.children;
        this.split_name = this.nodes_bak[0]?.name;
      } else { //reset, potreba hlavne kdyz se znovu nacitaji filtry (pri zmenen repo)
        this.nodes_bak = null;
        this.split_name = null;
      }
      this.hasChildren = !!this.nodes.find( (value, index, array)=> { return value.children || value.hasChildren } );
    }
    //this.selected = []; // muzem to vycistit? nebo zavolat prepareSelected() kdyztak
  }

  selectChange(event) {
    // pri zakliknuti parent nodu se toto vola pro kazdy jednotlivy prvek! takze neni vhodne, zde pokazde zpracovavat cely strom
    // to udelame az pri zavreni dropdownu
  }

  prepareSelected() {
    this.selected = [];
    this.tree.treeModel.clearFilter(); //zrus filter, protoze jinak plati jen to co zustalo vyfiltrovano (asi diky getVisibleChildren)
    this.filterfield.nativeElement.value='';

    // pripravime rekurzivni funkci
    function findFullSel(node: TreeNode) {
      if (node.isAllSelected) {
        this.selected.push(node);
      } else {
        node.getVisibleChildren().forEach( findFullSel, this );
      }
    }
    this.tree.treeModel.getVisibleRoots().forEach( findFullSel, this );
  }

  /* vrati podmnozinu stromu obsahujici jen vyselectovane casti */
  getSelectedNodeTree() {
    let level = 0;
    const treeFilter = (result: any[], node: TreeNode) => {
      if (node.isSelected == true) {
        let bak = level;
        level++;
        result.push({ id: node.data.id, name: node.data.nameCS, level: level,  children: node.getVisibleChildren().reduce( treeFilter, []) });
        level = bak;
      }
      return result;
    }

    return this.tree.treeModel.getVisibleRoots().reduce( treeFilter, [] );
  }


  dropOpened() {
    /* zda se, ze neni potreba. Resp je to potreba, kdyz se zapne useVirtualScroll: true,
     setTimeout(() => {
      console.log('timeout size');
      this.tree.sizeChanged();
    }, 1000); 
    */
  }
  
  getSelectedNodes() {
    return this.tree.treeModel.selectedLeafNodes;
  }

  getSelectedIds(): number[] {
    return Object.entries(this.tree.treeModel.selectedLeafNodeIds)
      .filter(([key, value]) => {
        return (value === true);
      }).map((node) => +node[0]);

  }

  getSelectedIdsWithInfo() {
    let result: any[] = [];
    const treeFilter = (level: number, path: any[], node: TreeNode ) => {
      //console.log(path);
      if (node.isSelected == true) {
        //path.push( { id: node.data.id, name: node.data.name } );
        
        let ch = node.getVisibleChildren();
        if (ch.length>0) {
          node.getVisibleChildren().forEach( (child: TreeNode) =>  { treeFilter(level+1, path.concat( { id: node.data.id, name: node.data.nameCS } ), child) }, this );
        } else {
          result.push({ id: node.data.id, name: node.data.nameCS, level: level, path: path } );
        }
      }
    }

    this.tree.treeModel.getVisibleRoots().forEach( (node: TreeNode) =>  { treeFilter(0, [], node) }, this );

    return result;
  }


  deselectAll() {
    // to je desne pomale
    //this.tree.treeModel.doForAll((node: TreeNode) => node.setIsSelected(false));
    this.tree.treeModel.selectedLeafNodes.forEach( (node: TreeNode) => node.setIsSelected(false) );
    this.prepareSelected();
  }


  public log(o) { console.log(o); }
}
