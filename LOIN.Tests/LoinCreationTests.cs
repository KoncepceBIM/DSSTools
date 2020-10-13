using LOIN.Context;
using LOIN.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Xbim.Common.Step21;
using Xbim.Ifc;
using Xbim.Ifc4.MeasureResource;

namespace LOIN.Tests
{
    [TestClass]
    public class LoinCreationTests
    {
        [TestMethod]
        public void BaseLoinCreationTest()
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
            var reason = loin.CreateReason("Handover", "Handover of data");
            var item = loin.CreateBreakedownItem("Window", "E456.789.12", "Window is a building element used to controll light flow into the space");



            var width = loin.CreateSimplePropertyTemplate("Width", "Width of the window", nameof(IfcLengthMeasure));
            width.SetName("cs", "Šířka");
            width.SetDescription("cs", "Šířka okna");

            Assert.IsNotNull(width.GetName("cs"));
            Assert.IsNotNull(width.GetDescription("cs"));

            var height = loin.CreateSimplePropertyTemplate("Height", "Height of the window", nameof(IfcLengthMeasure));
            height.SetName("cs", "Výška");
            height.SetDescription("cs", "Výška okna");
            var code = loin.CreateSimplePropertyTemplate("BarCode", "Bar code of the window", nameof(IfcIdentifier));
            code.SetName("cs", "Čárový kód");
            code.SetDescription("cs", "Čárový kód okna");

            var requirement = loin.CreatePropertySetTemplate("FM Requirements", "Requirements for Facility Management");
            requirement.HasPropertyTemplates.Add(width);
            requirement.HasPropertyTemplates.Add(height);
            requirement.HasPropertyTemplates.Add(code);
            requirement.SetName("cs", "Požadavky pro FM");
            requirement.SetDescription("cs", "Požadavky pro správu a údržbu");

            var requirements = loin.CreateRequirementSet("Base requirements", "Base requirements for the window");
            requirements.Add(requirement);

            var geomReq = loin.CreateGeometryRequirements();
            geomReq.ID = Guid.NewGuid().ToString();
            geomReq.Name = "Window geometry requirements";
            geomReq.Appearance = "No textures";
            geomReq.Definition = "Definition of the window geometry requirements";
            geomReq.Detailing = Requirements.DetailingEnum.LowDetail;
            geomReq.Dimensionality = Requirements.DimensionalityEnum.Dim_3D;
            geomReq.ParametricBehaviour = false;

            var context = new IContextEntity[] { actor, milestone, reason, item };
            foreach (var contextItem in context)
                contextItem.AddToContext(requirements);

            txn.Commit();

            // serialize as IFC
            loin.Save("LOIN.ifc");

            // serialize as XML
            loin.Save("LOIN.ifcXML");

            // validate IFC model
            var validator = new IfcValidator();
            var ok = validator.Check(loin.Internal);
            Assert.IsTrue(ok);

            // get MVD
            var mvd = loin.GetMvd(XbimSchemaVersion.Ifc4, "en", "LOIN Representation", "Requirements defined using LOIN, represented as validation MVD", "LOIN", "Classification");

            // serialize MVD
            mvd.Save("LOIN.mvdXML");

            // validate MVD XML XSD
            var logger = Xbim.Common.XbimLogging.CreateLogger("MVDXML schema validation");
            var msg = MvdValidator.ValidateXsd("LOIN.mvdXML", logger);
            Assert.IsTrue(string.IsNullOrWhiteSpace(msg));
        }

        [TestMethod]
        public void ConvertIfcLoinToMVD()
        {
            const string path = @"c:\Users\Martin\Dropbox (Personal)\xBIM.cz\Zakazky\@CAS\PS03\Datovy_standard\SW_Vendors\sample_20190809_1625.ifc";
            using var loin = Model.Open(path);
            var mvdPath = Path.ChangeExtension(path, ".mvdXML");
            var mvd = loin.GetMvd(XbimSchemaVersion.Ifc4, "cs", "Datový standard stavebnictví", "Validační MVD pro požadavky definované v DSS", "DSS", "Classification");
            mvd.Save(mvdPath);
            var log = Xbim.Common.XbimLogging.CreateLogger("MVD schema check");
            var err = MvdValidator.ValidateXsd(mvdPath, log);
            var ok = string.IsNullOrWhiteSpace(err);
            Assert.IsTrue(ok);
        }
    }
}
