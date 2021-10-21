using LOIN.Context;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Xbim.Ifc;
using Xbim.Ifc4.ExternalReferenceResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.MeasureResource;

namespace LOIN.Tests
{
    [TestClass]
    public class LangTests
    {
        private Model GetTestModel()
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

            var loin = Model.Create(editor);
            using var txn = loin.Internal.BeginTransaction("LOIN creation");

            var actor = loin.CreateActor("Client", "Owner of the building");
            var milestone = loin.CreateMilestone("Preliminary design", "Preliminary design handover milestone");
            var reason = loin.CreatePurpose("Handoved", "Handover of data");
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

            return loin;
        }

        [TestMethod]
        public void Property_template_should_have_CS_name()
        {
            var fileName = $"{Guid.NewGuid()}.ifc";
            
            {
                using var loin = GetTestModel();
                var p = loin.Internal.Instances.FirstOrDefault<IIfcPropertyTemplate>(p => p.Name == "Width");
                using var txn = loin.Internal.BeginTransaction("Localization");
                p.SetName("cs", "Šířka");
                p.SetDescription("cs", "Celková šířka včetně všech přesahů");
                p.SetDataTypeName("cs", "Šířka");
                txn.Commit();
                loin.Save(fileName);
            }

            {
                using var loin = Model.Open(fileName);
                var p = loin.Internal.Instances.FirstOrDefault<IIfcPropertyTemplate>(p => p.Name == "Width");

                var name = p.GetName("cs");
                var description = p.GetDescription("cs");
                var dataType = p.GetDataTypeName("cs");

                Assert.IsTrue(name == "Šířka");
                Assert.IsTrue(description == "Celková šířka včetně všech přesahů");
                Assert.IsTrue(dataType == "Šířka");

            }
        }

        [TestMethod]
        public void Breakdown_item_should_have_CS_name()
        {
            var fileName = $"{Guid.NewGuid()}.ifc";

            {
                using var loin = GetTestModel();
                var window = loin.Internal.Instances.FirstOrDefault<IfcClassificationReference>(p => p.Name == "Window");
                using var txn = loin.Internal.BeginTransaction("Localization");
                window.SetName("cs", "Okno");
                window.SetDescription("cs", "Taková ta díra ve zdi");
                txn.Commit();
                loin.Save(fileName);
            }

            {
                using var loin = Model.Open(fileName);
                var window = loin.Internal.Instances.FirstOrDefault<IfcClassificationReference>(p => p.Name == "Window");

                var name = window.GetName("cs");
                var description = window.GetDescription("cs");

                Assert.IsTrue(name == "Okno");
                Assert.IsTrue(description == "Taková ta díra ve zdi");

            }
        }
    }
}
