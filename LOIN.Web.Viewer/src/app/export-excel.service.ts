import { Injectable } from '@angular/core';

import { Workbook, Worksheet } from 'exceljs';
import * as fs from 'file-saver';


@Injectable({
  providedIn: 'root'
})
export class ExportExcelService {

  constructor() { }

  private columns = [
    { header: 'Cesta', key: 'path', width: 10 },
    { header: 'Skupina vlastností', key: 'setNameCS', width: 50 },
    { header: 'Název vlastnosti', key: 'nameCS', width: 50 },
    { header: 'Měrná jednotka', key: 'units', width: 10 },
    { header: 'Datový typ', key: 'dataTypeCS', width: 25 },
    { header: 'Popis', key: 'description', width: 50 },
    { header: 'Poznámka', key: 'examples', width: 20 },
    { header: 'kód (GUID)', key: 'uuid', width: 25 },
    { header: 'IFC Pset', key: 'setName', width: 25 },
    { header: 'IFC property', key: 'name', width: 20 },
    { header: 'IFC data type', key: 'dataType', width: 20 },
    { header: 'IFC type', key: 'valueType', width: 20 },
  ];

  private columnsTree = [
    { header: 'Cesta', key: 'path', width: 50 },
    //{ header: 'name', key: 'nameCS', width: 10 },
    { header: 'Popis', key: 'descriptionCS', width: 40 },
    { header: 'Poznámka', key: 'noteCS', width: 40 },
    { header: 'IFC_class', key: 'ifcType', width: 15 },
    { header: 'IFC_type', key: 'ifcPredefinedType', width: 15 },
    { header: 'CCI:SE', key: 'cciSE', width: 10 },
    { header: 'CCI:VF', key: 'cciVF', width: 10 },
    { header: 'CCI:FS', key: 'cciFS', width: 10 },
    { header: 'CCI:TS', key: 'cciTS', width: 10 },
    { header: 'CCI:KO', key: 'cciKO', width: 10 },
    { header: 'CCI:SK', key: 'cciSK', width: 10 },
    { header: 'kód (GUID)', key: 'uuid', width: 27 },
  ];


  private bgcolor_path="CCCCCC";//"DDDDDD";

  private exportRequirement(worksheet:Worksheet, data, level) {
    //console.log(dataTemplates[id]);

    data?.requirementSets.forEach( set => {
      // SetName
      var rowData = {};
      rowData['setNameCS']=set.name;
      var row = worksheet.addRow(rowData, "n");
      row.outlineLevel = level+1;
      //row.font = { bold: true };

      // Requirements
      set.requirements.forEach( req => {
        var rowData = {};
        this.columns.forEach( c => {
          if (req[c.key] && req[c.key].constructor === Array) {
            rowData[c.key]=req[c.key].join();
          } else {
            rowData[c.key]=req[c.key];
          }
        });
        //console.log(rowData);
  
        var row = worksheet.addRow(rowData, "n");
        row.outlineLevel = level+2;
        row.getCell(2).font = { color: { argb: 'FFFFFF' }, };
      });
    });
  }


  // funkce ktera rekurzivne projde cestu k finalnimu uzlu a nasledne vypise pozadavky
  private ExportPathToRequirements(worksheet:Worksheet, dataTemplates, node) {
    //console.info(node.id, node.name, node.level);
    var row = worksheet.addRow([ ' '.repeat(node.level) + ' ' + node.name ],'n');
    row.outlineLevel = node.level;
    row.border = {
      //top: {style: 'thin', color: {argb:this.bgcolor_path}},
      left: {style:'thin', color: {argb:this.bgcolor_path}},
      //bottom: {style:'thin', color: {argb:this.bgcolor_path}},
      right: {style:'thin', color: {argb:this.bgcolor_path}}
    };
    row.fill = { type: 'pattern', pattern:'solid', fgColor:{argb:this.bgcolor_path} };
    //row.font = { family:3 }; //family 1 - Serif, 2 - Sans Serif, 3 - Mono, Others - unknown
    if (node.children.length>0) {
      node.children.forEach( ch => this.ExportPathToRequirements(worksheet, dataTemplates, ch) );
    } else {
      this.exportRequirement(worksheet, dataTemplates[node.id], node.level);
    }
  }

  // funkce ktera rekurzivne projde cestu k finalnimu uzlu 
  private ExportPath(worksheet:Worksheet, dataTemplates, node) {
    var rowData = {};
    this.columnsTree.forEach( c => {
      if (dataTemplates[node.id]) rowData[c.key]=dataTemplates[node.id][c.key];
    });
    rowData['path']=' '.repeat(node.level) + ' ' + node.name;
    //console.log(node,rowData);
    var row = worksheet.addRow(rowData, "n");
    //row.font = { family:3 }; //family 1 - Serif, 2 - Sans Serif, 3 - Mono, Others - unknown
    if (node.children.length>0) {
      node.children.forEach( ch => this.ExportPath(worksheet, dataTemplates, ch) );
    } else {
      //this.exportRequirement(worksheet, dataTemplates[node.id], node.level);
      //row.getCell('path').fill = { type: 'pattern', pattern:'solid', fgColor:{argb:this.bgcolor_path} };
      row.getCell('path').font = { name: 'Calibri', family:2,  bold: true }; 
    }
  }


  private exportAddSheetRequirements(workbook:Workbook, name:string, dataTemplates:any[], tree:any[] ) {
    const worksheet:Worksheet = workbook.addWorksheet(name);

    worksheet.columns = this.columns;

    const rowheader = worksheet.getRow(1);
    rowheader.fill = { type: 'pattern', pattern:'solid', fgColor:{ argb:'AAAAAA' } };
    rowheader.font = { name: 'Calibri', family:2, bold: true }; 
    rowheader.alignment = { vertical: 'middle' }
    rowheader.height = 30;
  
    tree.forEach( node => this.ExportPathToRequirements(worksheet, dataTemplates, node) );
      

    worksheet.autoFilter = 'B1:Z1';
  }

  private exportAddSheetTree(workbook:Workbook, name:string, dataTemplates:any[], tree:any[] ) {
    const wstree:Worksheet = workbook.addWorksheet('Datové šablony');
    wstree.columns = this.columnsTree;

    const rowheader = wstree.getRow(1);
    rowheader.fill = { type: 'pattern', pattern:'solid', fgColor:{ argb:'AAAAAA' } };
    rowheader.font = { name: 'Calibri', family:2, bold: true }; 
    rowheader.alignment = { vertical: 'middle' }
    rowheader.height = 30;


    tree.forEach( node => this.ExportPath(wstree, dataTemplates, node) );
  }

  public export(dataTemplates:any[], tree:any[] ) {

    let workbook:Workbook = new Workbook();

    this.exportAddSheetRequirements(workbook, "Požadavky na vlastnosti", dataTemplates, tree);

    this.exportAddSheetTree(workbook, 'Datové šablony', dataTemplates, tree);


    workbook.xlsx.writeBuffer().then((data) => {
      let blob = new Blob([data], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
      fs.saveAs(blob, 'Datový standard staveb.xlsx');
    })

  
  }





/* ukazka jak stahnout XLS (sablonu) ze serveru a tu nacist
    //this.httpClient.get('assets/images/dss.xlsx', { responseType: 'blob' }).subscribe(async (data) => {
      this.httpClient.get('assets/images/dss.xlsx', { responseType: 'arraybuffer' }).subscribe( {
        next: (xls) => {
          let workbook = new Workbook();
          await workbook.xlsx.load( xls );
        }
        error: (err) => {
          window.alert('Internal error: Nepodařilo se načíst XLS šablonu');
        }
      });
*/

}
