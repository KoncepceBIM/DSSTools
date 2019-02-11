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

namespace DDSS
{
    class Program
    {
        static void Main(string[] args)
        {
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

            IfcStore.ModelProviderFactory.UseMemoryModelProvider();
            using (var model = IfcStore.Create(editor, XbimSchemaVersion.Ifc4, XbimStoreType.InMemoryModel))
            { 
                var i = model.Instances;
                using (var txn = model.BeginTransaction())
                {
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
                        rel.RelatingControl = i.New<IfcActionRequest>(r =>
                        {
                            r.Name = "Kontrola zadaných parametrů";
                            r.ObjectType = "INFORMATION_REQUEST";
                            r.PredefinedType = IfcActionRequestTypeEnum.USERDEFINED;
                        });
                    });

                    // Actor / Role = Who in interested in the data
                    i.New<IfcRelAssignsToActor>(r =>
                    {
                        r.ActingRole = i.New<IfcActorRole>(ar => ar.Role = IfcRoleEnum.CLIENT);
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
                        rel.RelatingClassification =
                            i.New<IfcClassificationReference>(c => {
                                c.Identification = "801 31";
                                c.Name = "Budovy mateřských škol";
                                c.ReferencedSource = i.New<IfcClassification>(cs => {
                                    cs.Name = "JKSO";
                                    cs.Description = "Klasifikace stavebních objektů";
                                    cs.Source = "https://www.cs-urs.cz/ciselniky-online/jkso/?cil=8013";
                                });
                            });
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
                                i.New<IfcSimplePropertyTemplate>(p => {
                                    p.Name = "Název";
                                    p.Description = "Unikátní název místnosti. Pokud je budova rozdělena na bloky, je první písmeno označující blok budovy. Následující číslice obsahují podlaží a číslo místnosti.";
                                    p.PrimaryMeasureType = nameof(IfcIdentifier);
                                    p.TemplateType = IfcSimplePropertyTemplateTypeEnum.P_SINGLEVALUE;
                                    
                                }),
                                i.New<IfcSimplePropertyTemplate>(p => {
                                    p.Name = "Plocha";
                                    p.PrimaryMeasureType = nameof(IfcAreaMeasure);
                                    p.PrimaryUnit = i.New<IfcSIUnit>(t => {
                                        t.Name = IfcSIUnitName.SQUARE_METRE;
                                        t.UnitType = IfcUnitEnum.AREAUNIT;
                                    });
                                    p.TemplateType = IfcSimplePropertyTemplateTypeEnum.P_SINGLEVALUE;
                                    i.New<IfcRelAssociatesDocument>(rd => {
                                        rd.RelatedObjects.Add(p);
                                        rd.RelatingDocument = i.New<IfcDocumentReference>(doc => {
                                            doc.Identification = "Vyhláška č. 441/2013 Sb.";
                                            doc.Location = "https://www.mfcr.cz/cs/legislativa/legislativni-dokumenty/2013/vyhlaska-c-441-2013-sb-16290";
                                            doc.Name = "Vyhláška k provedení zákona o oceňování majetku (oceňovací vyhláška)";

                                        });
                                    });
                                })
                                // .... add all properties here
                            });
                        }));
                        // ... add all property sets here
                    });

                    txn.Commit();
                }

                model.SaveAs("DDSS.ifc");
                //model.SaveAs("DDSS.ifcxml");
            }
        }
    }
}
