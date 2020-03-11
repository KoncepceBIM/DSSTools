using LOIN.Context;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xbim.Ifc;
using Xbim.Ifc4.ExternalReferenceResource;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.MeasureResource;
using Xbim.Ifc4.PropertyResource;
using Xbim.Ifc4.SharedBldgElements;
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
            var mvd = loin.GetMvd("en", "LOIN Representation", "Requirements defined using LOIN, represented as validation MVD", "LOIN", "Classification");
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
            var window = i.New<IfcWindow>(w => {
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
    }
}
