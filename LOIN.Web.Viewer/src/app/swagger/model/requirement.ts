/**
 * LOIN.Server
 * No description provided (generated by Swagger Codegen https://github.com/swagger-api/swagger-codegen)
 *
 * OpenAPI spec version: 1.0
 * 
 *
 * NOTE: This class is auto generated by the swagger code generator program.
 * https://github.com/swagger-api/swagger-codegen.git
 * Do not edit the class manually.
 */

export interface Requirement { 
    id?: number;
    uuid?: string;
    name?: string;
    nameCS?: string;
    nameEN?: string;
    description?: string;
    descriptionCS?: string;
    descriptionEN?: string;
    identifier?: string;
    units?: string;
    valueType?: string;
    dataType?: string;
    dataTypeCS?: string;
    dataTypeEN?: string;
    setName?: string;
    setNameCS?: string;
    setNameEN?: string;
    setDescription?: string;
    setDescriptionCS?: string;
    setDescriptionEN?: string;
    noteCS?: string;
    noteEN?: string;
    enumeration?: Array<string>;
    examples?: Array<string>;
}