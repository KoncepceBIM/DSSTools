using DDSS.Utils;
using Serilog;
using System;
using System.IO;
using Xbim.Common;
using Xbim.Common.Step21;
using Xbim.Ifc;
using Xbim.Ifc4.ActorResource;
using Xbim.Ifc4.ExternalReferenceResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.MeasureResource;
using Xbim.Ifc4.ProcessExtension;
using Xbim.Ifc4.ProductExtension;
using Xbim.Ifc4.SharedMgmtElements;
using Xbim.IO;
using Xbim.IO.Memory;

namespace DDSS
{
    class Program
    {
        static void Main(string[] args)
        {
            const string ifcFile = "DDSS.ifc";

            var editor = new XbimEditorCredentials
            {
                ApplicationFullName = "Databáze datového standardu stavebnictví",
                ApplicationDevelopersName = "Michal Kopecký",
                ApplicationIdentifier = "DDSS",
                ApplicationVersion = "1.0",
                EditorsFamilyName = "Žák",
                EditorsGivenName = "Josef",
                EditorsOrganisationName = "Česká agentura pro standardizaci"
            };

            // set up logger to file and console
            var logFile = Path.ChangeExtension(ifcFile, ".log");
            Log.Logger = new LoggerConfiguration()
               .Enrich.FromLogContext()
               .WriteTo.Console()
               .WriteTo.File(logFile)
               .CreateLogger();
            XbimLogging.LoggerFactory.AddSerilog();

            using (var model = new MemoryModel(new Xbim.Ifc4.EntityFactoryIfc4()))
            { 
                var i = model.Instances;
                using (var txn = model.BeginTransaction("Model creation"))
                {
                    // hook event listeners to set up GUIDs and owner history objects
                    model.Init(editor);

                    // create classification hierarchy
                    var classificationMap = ClassificationImporter.ImportInto(model);

                    // these will keep cache of units and enums
                    // units have to be defined in the source of 'UnitMap' (compile time) while
                    // enumerations are to be defined in runtime based on the values in the DB
                    var units = new UnitMap(model);
                    var enums = new EnumsMap(model);

                    // LOIN = level of information need for specific purpose, role, milestone and classification
                    // root object type, there might be many of them
                    var loin = i.New<IfcProjectLibrary>(lib =>
                    {
                        lib.Name = "Level of Information Need";
                    });

                    // Purpose of the data requirement/exchange
                    i.New<IfcRelAssignsToControl>(rel =>
                    {
                        rel.RelatedObjects.Add(loin);
                        rel.RelatedObjectsType = IfcObjectTypeEnum.NOTDEFINED;
                        rel.RelatingControl = i.New<IfcActionRequest>(r =>
                        {
                            r.Name = "Kontrola zadaných parametrů";
                            r.ObjectType = "INFORMATION_REQUEST";
                            r.PredefinedType = IfcActionRequestTypeEnum.USERDEFINED;
                        });
                    });

                    // Actor / Role = Who is interested in the data
                    i.New<IfcRelAssignsToActor>(r =>
                    {
                        r.ActingRole = i.New<IfcActorRole>(ar => ar.Role = IfcRoleEnum.CLIENT);
                        r.RelatedObjectsType = IfcObjectTypeEnum.NOTDEFINED;
                        r.RelatedObjects.Add(loin);
                        r.RelatingActor = i.New<IfcActor>(a =>
                        {
                            a.TheActor = i.New<IfcOrganization>(p =>
                            {
                                p.Name = "Veřejný zadavatel";
                            });
                        });
                    });

                    // Milestone = point in time
                    i.New<IfcRelAssignsToProcess>(rel =>
                    {
                        rel.RelatedObjects.Add(loin);
                        rel.RelatedObjectsType = IfcObjectTypeEnum.NOTDEFINED;
                        rel.RelatingProcess = i.New<IfcTask>(t =>
                        {
                            t.Name = "Stavební povolení";
                            t.IsMilestone = true;
                        });
                    });


                    // Classification = subject of interest
                    i.New<IfcRelAssociatesClassification>(rel =>
                    {
                        rel.RelatedObjects.Add(loin);
                        // 5006 would be a database identifier of the original classification item
                        rel.RelatingClassification = classificationMap[5006];
                    });

                    // Declared data requirements / templates
                    i.New<IfcRelDeclares>(decl =>
                    {
                        decl.RelatingContext = loin;
                        decl.RelatedDefinitions.Add(i.New<IfcPropertySetTemplate>(ps =>
                        {
                            ps.Name = "Základní informace o místnostech";
                            ps.ApplicableEntity = nameof(IfcSpace);
                            ps.HasPropertyTemplates.AddRange(new[] {
                                // Simple property without unit
                                i.New<IfcSimplePropertyTemplate>(p => {
                                    p.Name = "Název";
                                    p.Description = "Unikátní název místnosti. Pokud je budova rozdělena na bloky, je první písmeno označující blok budovy. Následující číslice obsahují podlaží a číslo místnosti.";
                                    p.PrimaryMeasureType = nameof(IfcIdentifier);
                                    p.TemplateType = IfcSimplePropertyTemplateTypeEnum.P_SINGLEVALUE;
                                    
                                }),
                                // simple property with unit m² and referenced document. There may be many documents as well
                                i.New<IfcSimplePropertyTemplate>(p => {
                                    p.Name = "Plocha";
                                    p.PrimaryMeasureType = nameof(IfcAreaMeasure);
                                    p.PrimaryUnit = units["m2"];     //      <----------  All units have to be defined in compile time in the code
                                    p.TemplateType = IfcSimplePropertyTemplateTypeEnum.P_SINGLEVALUE;
                                    i.New<IfcRelAssociatesDocument>(rd => {
                                        rd.RelatedObjects.Add(p);
                                        rd.RelatingDocument = i.New<IfcDocumentReference>(doc => {
                                            doc.Identification = "Vyhláška č. 441/2013 Sb.";
                                            doc.Location = "https://www.mfcr.cz/cs/legislativa/legislativni-dokumenty/2013/vyhlaska-c-441-2013-sb-16290";
                                            doc.Name = "Vyhláška k provedení zákona o oceňování majetku (oceňovací vyhláška)";

                                        });
                                    });
                                }),

                                // simple property with complex unit W / m² · K
                                i.New<IfcSimplePropertyTemplate>(p => {
                                    p.Name = "Thermal admittance";
                                    p.PrimaryMeasureType = nameof(IfcThermalAdmittanceMeasure);
                                    p.PrimaryUnit = units["W/m2.K"];
                                    p.TemplateType = IfcSimplePropertyTemplateTypeEnum.P_SINGLEVALUE;
                                }),

                                // property enumeration
                                i.New<IfcSimplePropertyTemplate>(p => {
                                    p.Name = "Den konání";
                                    p.PrimaryMeasureType = nameof(IfcLabel);  //    <------------- just use this for enumerations
                                    p.TemplateType = IfcSimplePropertyTemplateTypeEnum.P_ENUMERATEDVALUE;  //   <----- this indicates it is an enumeration
                                    p.Enumerators = enums.GetOrAdd("Dny v týdnu", new [] { "Pondělí", "Úterý", "Středa", "Čtvrtek", "Pátek", "Sobota", "Neděle"});  //      <--------values from DB
                                    // or use p.Enumerators = enums.GetOrAdd("987jfgh-8978jfgh-jghjf45", "Dny v týdnu", new [] { "Pondělí", "Úterý", "Středa", "Čtvrtek", "Pátek", "Sobota", "Neděle"});  //      <--------values from DB
                                    // if the NAME IS NOT THE KEY. You can also add all enumerations somewhere else and only get them by key here: enums["987jfgh-8978jfgh-jghjf45"]
                                })

                                // .... add all properties here
                            });
                        }));
                        // ... add all property sets here
                    });

                    txn.Commit();
                }

                // validate schema and proper units
                var validator = new Validator();
                var valid = validator.Check(model);
                if (!valid)
                    throw new Exception("Invalid model shouldn't be stored and used for any purposes.");

                // this is stdout if you want to use it
                var stdout = Console.OpenStandardOutput();

                // writing to stream (any stream)
                using (var stream = File.Create(ifcFile))
                {
                    model.SaveAsIfc(stream);
                }


                // var xmlFile = Path.ChangeExtension(ifcFile, ".ifcxml");
                // using (var stream = File.Create(xmlFile))
                // {
                //     model.SaveAsIfcXml(stream);
                // }
            }
        }
    }
}
