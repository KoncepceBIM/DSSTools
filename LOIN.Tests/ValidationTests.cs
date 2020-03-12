using LOIN.Context;
using LOIN.Validation;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xbim.Common.Step21;
using Xbim.Ifc;
using Xbim.Ifc4.ExternalReferenceResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.MeasureResource;
using Xbim.Ifc4.PropertyResource;
using Xbim.Ifc4.SharedBldgElements;
using Xbim.IO.Memory;
using Xbim.MvdXml;
using Xbim.MvdXml.DataManagement;

namespace LOIN.Tests
{
    [TestClass]
    public class ValidationTests
    {
        [TestMethod]
        public void ValidateSimpleLoinTest()
        {
            var editor = new XbimEditorCredentials
            {
                ApplicationDevelopersName = "CAS",
                ApplicationFullName = "LOIN Creator",
                ApplicationIdentifier = "LOIN",
                ApplicationVersion = "4.0",
                EditorsFamilyName = "Santini Aichel",
                EditorsGivenName = "Johann Blasius",
                EditorsOrganisationName = "Independent Architecture"
            };

            using var loin = Model.Create(editor);
            using var txn = loin.Internal.BeginTransaction("LOIN creation");

            var actor = loin.CreateActor("Client", "Owner of the building");
            var milestone = loin.CreateMilestone("Preliminary design", "Preliminary design handover milestone");
            var reason = loin.CreateReason("Handoved", "Handover of data");
            var item = loin.CreateBreakedownItem("Window", "E456.789.12", "Window is a building element used to controll light flow into the space");



            var width = loin.CreateSimplePropertyTemplate("Width", "Width of the window", nameof(IfcLengthMeasure));
            var height = loin.CreateSimplePropertyTemplate("Height", "Height of the window", nameof(IfcLengthMeasure));
            var code = loin.CreateSimplePropertyTemplate("BarCode", "Bar code of the window", nameof(IfcIdentifier));

            var requirement = loin.CreatePropertySetTemplate("FM Requirements", "Requirements for Facility Management");
            requirement.HasPropertyTemplates.Add(width);
            requirement.HasPropertyTemplates.Add(height);
            requirement.HasPropertyTemplates.Add(code);

            var requirements = loin.CreateRequirementSet("Base requirements", "Base requirements for the window");
            requirements.Add(requirement);

            var context = new IContextEntity[] { actor, milestone, reason, item };
            foreach (var contextItem in context)
                contextItem.AddToContext(requirements);

            txn.Commit();

            // get MVD
            var mvd = loin.GetMvd(XbimSchemaVersion.Ifc4, "en", "LOIN Representation", "Requirements defined using LOIN, represented as validation MVD", "LOIN", "Classification");
            mvd.Save("MinimalWindow.mvdXML");

            using var model = IfcStore.Create(editor, Xbim.Common.Step21.XbimSchemaVersion.Ifc4, Xbim.IO.XbimStoreType.InMemoryModel);
            using var creationTxn = model.BeginTransaction();
            var i = model.Instances;

            var clsRel = i.New<IfcRelAssociatesClassification>(r =>
            {
                r.RelatingClassification = i.New<IfcClassificationReference>(c =>
                {
                    c.Name = "Window";
                    c.Identification = "E456.789.12";
                });
            });
            var propsRel = i.New<IfcRelDefinesByProperties>(r =>
            {
                r.RelatingPropertyDefinition = i.New<IfcPropertySet>(pset =>
                {
                    pset.Name = "FM Requirements";
                    pset.HasProperties.AddRange(new[]
                    {
                        i.New<IfcPropertySingleValue>(p =>
                        {
                            p.Name = "Width";
                            p.NominalValue = new IfcLengthMeasure(12.45);
                        }),
                        i.New<IfcPropertySingleValue>(p =>
                        {
                            p.Name = "Height";
                            p.NominalValue = new IfcLengthMeasure(78.65);
                        }),
                        i.New<IfcPropertySingleValue>(p =>
                        {
                            p.Name = "BarCode";
                            p.NominalValue = new IfcIdentifier("987fsd-54fsdf6456-dsf65464-fs456");
                        })
                    });
                });
            });
            var window = i.New<IfcWindow>(w =>
            {
                w.Name = "Window #1";
            });
            clsRel.RelatedObjects.Add(window);
            propsRel.RelatedObjects.Add(window);


            creationTxn.Commit();
            model.SaveAs("MinimalWindow.ifc");

            var engine = new MvdEngine(mvd, model);
            var conceptRoot = engine.ConceptRoots.FirstOrDefault();
            Assert.IsTrue(conceptRoot.AppliesTo(window));

            foreach (var concept in conceptRoot.Concepts)
            {
                var passes = concept.Test(window, Xbim.MvdXml.Concept.ConceptTestMode.Raw);
                Assert.IsTrue(passes == ConceptTestResult.Pass);
            }
        }

        [TestMethod]
        public void ValidateShed()
        {
            const string mvdFile = "Files/CCI_for_doors.mvdXML";
            const string ifcFile = "Files/Bouda.ifc";

            using (var file = File.OpenRead(ifcFile))
            {
                var mvd = mvdXML.Deserialize(File.ReadAllText(mvdFile));
                var result = MvdValidator.ValidateModel(mvd, file, NullLogger.Instance);
                // nothing matches applicability selectors
                Assert.IsTrue(result.All(r => r.Concept == null && r.Result == ConceptTestResult.DoesNotApply));
            }

            var classified = Guid.NewGuid() + ".ifc";
            var doorCount = 0;
            using (var model = MemoryModel.OpenRead(ifcFile, null))
            {
                doorCount = model.Instances.OfType<IIfcDoor>().Count(); ;
                using (var txn = model.BeginTransaction("Classification"))
                {
                    var c = new Create(model);
                    c.RelDefinesByProperties(r =>
                    {
                        r.RelatingPropertyDefinition = c.PropertySet(ps =>
                        {
                            ps.Name = "CZ_DataTemplateDesignation";
                            ps.HasProperties.Add(c.PropertySingleValue(p =>
                            {
                                p.Name = "DataTemplate ID";
                                p.NominalValue = new IfcLabel("52459");
                            }));
                        });
                        r.RelatedObjects.AddRange(model.Instances.OfType<IIfcDoor>());
                    });
                    txn.Commit();
                }

                using var file = File.Create(classified);
                model.SaveAsIfc(file);
            }

            using (var file = File.OpenRead(classified))
            {
                var mvd = mvdXML.Deserialize(File.ReadAllText(mvdFile));
                var result = MvdValidator.ValidateModel(mvd, file, NullLogger.Instance);
                var doorResults = result.Where(r => r.Entity is IIfcDoor).ToList();
                Assert.AreEqual(doorCount, doorResults.Count);
                Assert.IsTrue(doorResults.All(r => r.Result == ConceptTestResult.Fail));
            }

            var withCCI = Guid.NewGuid() + ".ifc";
            using (var model = MemoryModel.OpenRead(classified, null))
            {
                using (var txn = model.BeginTransaction("Classification"))
                {
                    var c = new Create(model);
                    c.RelDefinesByProperties(r =>
                    {
                        r.RelatingPropertyDefinition = c.PropertySet(ps =>
                        {
                            ps.Name = "CZ_ClassificationSystemCCI";
                            ps.HasProperties.AddRange(new[] {
                                c.PropertySingleValue(p =>
                                {
                                    p.Name = "CCICode";
                                    p.NominalValue = new IfcLabel("XXX");
                                }),
                                c.PropertySingleValue(p =>
                                {
                                    p.Name = "FunctionalSystem";
                                    p.NominalValue = new IfcLabel("XXX");
                                }),
                                c.PropertySingleValue(p =>
                                {
                                    p.Name = "ContructiveSystem";
                                    p.NominalValue = new IfcLabel("XXX");
                                }),
                                c.PropertySingleValue(p =>
                                {
                                    p.Name = "CodeComponent";
                                    p.NominalValue = new IfcLabel("XXX");
                                })
                            });
                        });
                        r.RelatedObjects.AddRange(model.Instances.OfType<IIfcDoor>());
                    });
                    txn.Commit();
                }

                using var file = File.Create(withCCI);
                model.SaveAsIfc(file);
            }

            using (var file = File.OpenRead(withCCI))
            {
                var mvd = mvdXML.Deserialize(File.ReadAllText(mvdFile));
                var result = MvdValidator.ValidateModel(mvd, file, NullLogger.Instance);
                var doorResults = result.Where(r => r.Entity is IIfcDoor).ToList();
                Assert.AreEqual(doorCount, doorResults.Count);
                Assert.IsTrue(doorResults.All(r => r.Result == ConceptTestResult.Pass));
            }
        }
    }
}
