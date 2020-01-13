using Serilog;
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
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

using CsvHelper;
using LOIN.Validation;
using LOIN.Context;
using LOIN.Requirements;

namespace LOIN.Exporter
{
    class xBimIfcLine
    {
        public int Id { get; set; }        // Line ID
        public int Pid { get; set; }       // Parent Line ID
        public int Lvl { get; set; }       // Row level -2 .. +inf
        public string Method { get; set; } // Name of method
        public string GlobalId { get; set; } // GUID
        public string Par01 { get; set; }
        public string Par02 { get; set; }
        public string Par03 { get; set; }
        public string Par04 { get; set; }
        public string Par05 { get; set; }
        public string Par06 { get; set; }
        public string Par07 { get; set; }
        public string Par08 { get; set; }
        public string Par09 { get; set; }
    }

    class Program
    {
        // LOIN = level of information need for specific purpose, role, milestone and classification
        // root object type, there might be many of them
        static RequirementsSet currentRequirementsSet; // loin
        //
        static Reason currentReason;
        static Actor currentActor;
        static Milestone currentMilestone;
        static Dictionary<string, BreakedownItem> breakedownRootMap;
        static BreakedownItem currentBreakedownItem;
        static IfcPropertySetTemplate currentPropertySet;
        static IfcSimplePropertyTemplate currentPropertyTemplate;
        //Console.WriteLine(ifcPL.GetType());

        //https://www.dotnetperls.com/map
        static Dictionary<string, IfcSIUnitName> ifcSIUnitMap;
        static Dictionary<int, BreakedownItem> breakedownMap; // mapa trid pouzitych v modelu
        static Dictionary<int, IfcSimplePropertyTemplate> ifcPropertyMap; // mapa vlastnosti pouzitych v modelu
        static Dictionary<int, IfcPropertySetTemplate> ifcPropertySetMap; // mapa skupin vlastnosti pouzitych v modelu
        static Dictionary<int, Reason> reasonsMap; // mapa IfcRelAssignsToControl pouzitych v modelu
        static Dictionary<string, Actor> actorMap; // mapa IfcRelAssignsToActor pouzitych v modelu
        static Dictionary<int, Milestone> ifcProcessMap; // mapa IfcRelAssignsToProcess pouzitych v modelu

        static bool PropertySetReused;

        static void Main(string[] args)
        {
            //
            ifcSIUnitMap = new Dictionary<string, IfcSIUnitName>();
            ifcSIUnitMap.Add("AMPERE", IfcSIUnitName.AMPERE);
            ifcSIUnitMap.Add("BECQUEREL", IfcSIUnitName.BECQUEREL);
            ifcSIUnitMap.Add("CANDELA", IfcSIUnitName.CANDELA);
            ifcSIUnitMap.Add("COULOMB", IfcSIUnitName.COULOMB);
            ifcSIUnitMap.Add("CUBIC_METRE", IfcSIUnitName.CUBIC_METRE);
            ifcSIUnitMap.Add("DEGREE_CELSIUS", IfcSIUnitName.DEGREE_CELSIUS);
            ifcSIUnitMap.Add("FARAD", IfcSIUnitName.FARAD);
            ifcSIUnitMap.Add("GRAM", IfcSIUnitName.GRAM);
            ifcSIUnitMap.Add("GRAY", IfcSIUnitName.GRAY);
            ifcSIUnitMap.Add("HENRY", IfcSIUnitName.HENRY);
            ifcSIUnitMap.Add("HERTZ", IfcSIUnitName.HERTZ);
            ifcSIUnitMap.Add("JOULE", IfcSIUnitName.JOULE);
            ifcSIUnitMap.Add("KELVIN", IfcSIUnitName.KELVIN);
            ifcSIUnitMap.Add("LUMEN", IfcSIUnitName.LUMEN);
            ifcSIUnitMap.Add("LUX", IfcSIUnitName.LUX);
            ifcSIUnitMap.Add("METRE", IfcSIUnitName.METRE);
            ifcSIUnitMap.Add("MOLE", IfcSIUnitName.MOLE);
            ifcSIUnitMap.Add("NEWTON", IfcSIUnitName.NEWTON);
            ifcSIUnitMap.Add("OHM", IfcSIUnitName.OHM);
            ifcSIUnitMap.Add("PASCAL", IfcSIUnitName.PASCAL);
            ifcSIUnitMap.Add("RADIAN", IfcSIUnitName.RADIAN);
            ifcSIUnitMap.Add("SECOND", IfcSIUnitName.SECOND);
            ifcSIUnitMap.Add("SIEMENS", IfcSIUnitName.SIEMENS);
            ifcSIUnitMap.Add("SIEVERT", IfcSIUnitName.SIEVERT);
            ifcSIUnitMap.Add("SQUARE_METRE", IfcSIUnitName.SQUARE_METRE);
            ifcSIUnitMap.Add("STERADIAN", IfcSIUnitName.STERADIAN);
            ifcSIUnitMap.Add("TESLA", IfcSIUnitName.TESLA);
            ifcSIUnitMap.Add("VOLT", IfcSIUnitName.VOLT);
            ifcSIUnitMap.Add("WATT", IfcSIUnitName.WATT);
            ifcSIUnitMap.Add("WEBER", IfcSIUnitName.WEBER);
            //
            breakedownRootMap = new Dictionary<string, BreakedownItem>(); // prazdna mapa klasifikaci, bude plnena postupne
            //
            breakedownMap = new Dictionary<int, BreakedownItem>(); // prazdna mapa trid, bude plnena postupne
            ifcPropertyMap = new Dictionary<int, IfcSimplePropertyTemplate>(); // prazdna mapa vlastnosti, bude plnena postupne
            ifcPropertySetMap = new Dictionary<int, IfcPropertySetTemplate>(); // prazdna mapa skupin vlastnosti, bude plnena postupne
            reasonsMap = new Dictionary<int, Reason>(); // prazdna mapa IfcRelAssignsToControl, bude plnena postupne
            actorMap = new Dictionary<string, Actor>(); // prazdna mapa IfcRelAssignsToActor, bude plnena postupne
            ifcProcessMap = new Dictionary<int, Milestone>(); // prazdna mapa IfcRelAssignsToProcess, bude plnena postupne

            //using (var reader = new StreamReader("xBimFile.csv"))
            using (var reader = new StreamReader(Console.OpenStandardInput()))
            using (var csv = new CsvReader(reader))
            {

                var records = new List<xBimIfcLine>();
                csv.Read(); // CSV header Line 0 - XbimEditorCredentials
                csv.ReadHeader();

                csv.Read(); // CSV line 0 - XbimEditorCredentials
                var record = new xBimIfcLine
                {
                    Id = csv.GetField<int>("Id"),
                    Pid = csv.GetField<int>("Pid"),
                    Lvl = csv.GetField<int>("Lvl") + 2, // -2..+inf -> 0..+inf
                    Method = csv.GetField("Method"),
                    GlobalId = csv.GetField("GlobalId"),
                    Par01 = csv.GetField("Par01"),
                    Par02 = csv.GetField("Par02"),
                    Par03 = csv.GetField("Par03"),
                    Par04 = csv.GetField("Par04"),
                    Par05 = csv.GetField("Par05"),
                    Par06 = csv.GetField("Par06"),
                    Par07 = csv.GetField("Par07"),
                    Par08 = csv.GetField("Par08"),
                    Par09 = csv.GetField("Par09")
                };
                records.Add(record);

                //START
                var ifcFile = args[0]; // tempfile // "/var/www/html/tmp/DDSS.ifc";
                var doValidate = "validate";
                if (args.Length > 1)
                    doValidate = args[1]; // "novalidate"

                var editor = new XbimEditorCredentials
                {
                    ApplicationFullName = record.Par01, // "Databáze datového standardu stavebnictví"
                    ApplicationDevelopersName = record.Par02, // "Michal Kopecký"
                    ApplicationIdentifier = record.Par03, // "DDSS"
                    ApplicationVersion = record.Par04, // "1.0"
                    EditorsFamilyName = record.Par05, // "Žák"
                    EditorsGivenName = record.Par06, // "Josef"
                    EditorsOrganisationName = record.Par07, // "Česká agentura pro standardizaci"
                };

                //IfcStore.ModelProviderFactory.UseMemoryModelProvider(); // OLD VERSION
                // set up logger to file and console
                var logFile = Path.ChangeExtension(ifcFile, ".log");
                Log.Logger = new LoggerConfiguration()
                   .Enrich.FromLogContext()
                   .WriteTo.Console()
                   .WriteTo.File(logFile)
                   .CreateLogger();
                XbimLogging.LoggerFactory.AddSerilog();

                //using (var model = IfcStore.Create(editor, XbimSchemaVersion.Ifc4, XbimStoreType.InMemoryModel)) // OLD VERSION
                using (var model = Model.Create(editor))
                {
                    //var i = model.Internal.Instances;

                    //using (var txn = model.BeginTransaction()) // OLD VERSION
                    using (var txn = model.Internal.BeginTransaction("Model creation"))
                    {

                        // these will keep cache of units and enums
                        // units have to be defined in the source of 'UnitMap' (compile time) while
                        // enumerations are to be defined in runtime based on the values in the DB
                        var units = new UnitMap(model);
                        var enums = new EnumsMap(model);

                        while (csv.Read())
                        {
                            record = new xBimIfcLine
                            {
                                Id = csv.GetField<int>("Id"),
                                Pid = csv.GetField<int>("Pid"),
                                Method = csv.GetField("Method"),
                                GlobalId = csv.GetField("GlobalId"),
                                Par01 = csv.GetField("Par01"),
                                Par02 = csv.GetField("Par02"),
                                Par03 = csv.GetField("Par03"),
                                Par04 = csv.GetField("Par04"),
                                Par05 = csv.GetField("Par05"),
                                Par06 = csv.GetField("Par06"),
                                Par07 = csv.GetField("Par07"),
                                Par08 = csv.GetField("Par08"),
                                Par09 = csv.GetField("Par09")
                            };
                            records.Add(record);
                            //Console.WriteLine(record.Method);

                            if (record.Method == "IfcClassification")
                            {
                                var root = model.CreateBreakedownRoot(record.Par01, null); // "Klasifikace DSS, CCI:ET, CCI:CS, CCI:FS"
                                breakedownRootMap.Add(record.Par01, root);
                            }
                            else if (record.Method == "IfcProjectLibrary")
                            {
                                currentRequirementsSet = model.CreateRequirementSet(record.Par01, null); // "Level of Information Need"
                                if (record.GlobalId != "")
                                    currentRequirementsSet.Entity.GlobalId = record.GlobalId;
                            }
                            else if (record.Method == "IfcRelAssignsToControl")
                            {
                                // Purpose of the data requirement/exchange
                                if (reasonsMap.ContainsKey(record.Id))
                                {
                                    currentReason = reasonsMap[record.Id];
                                }
                                else
                                {
                                    currentReason = model.CreateReason(record.Par01, record.Par02);
                                    currentReason.AddToContext(currentRequirementsSet);
                                    if (record.GlobalId != "")
                                        currentReason.Entity.GlobalId = record.GlobalId;
                                    reasonsMap.Add(record.Id, currentReason);
                                };
                            }
                            else if (record.Method == "IfcRelAssignsToActor")
                            {
                                // Actor / Role = Who is interested in the data
                                if (actorMap.ContainsKey(record.GlobalId))
                                {
                                    currentActor = actorMap[record.GlobalId];
                                }
                                else
                                {
                                    currentActor = model.CreateActor(record.Par01, null);
                                    currentActor.AddToContext(currentRequirementsSet);
                                    if (record.GlobalId != "")
                                        currentActor.Entity.GlobalId = record.GlobalId;
                                    actorMap.Add(record.GlobalId, currentActor);
                                };
                            }
                            else if (record.Method == "IfcRelAssignsToProcess")
                            {
                                // Milestone = point in time
                                if (ifcProcessMap.ContainsKey(record.Id))
                                {
                                    currentMilestone = ifcProcessMap[record.Id];
                                }
                                else
                                {
                                    currentMilestone = model.CreateMilestone(record.Par01, null);
                                    currentMilestone.AddToContext(currentRequirementsSet);
                                    currentMilestone.Entity.IsMilestone = record.Par02 == "true";

                                    if (record.GlobalId != "")
                                        currentMilestone.Entity.GlobalId = record.GlobalId;
                                    ifcProcessMap.Add(record.Id, currentMilestone);
                                };
                            }
                            else if (record.Method == "IfcClassificationReference")
                            {
                                // Class within classification
                                // Set parent
                                BreakedownItem parent = null;
                                if (record.Par03 == "")
                                    //Program.ifcCRS = Program.ifcCL; // classification for root class
                                    parent = breakedownRootMap[record.Par09]; // classification for root class
                                else
                                    parent = breakedownMap[record.Pid]; // parent class othewise
                                // Optionally add new class
                                if (!breakedownMap.ContainsKey(record.Id))
                                {
                                    currentBreakedownItem = model.CreateBreakedownItem(record.Par01, record.Par02, null, parent);
                                    breakedownMap.Add(record.Id, currentBreakedownItem);
                                };
                            }
                            else if (record.Method == "IfcRelAssociatesClassification")
                            {
                                breakedownMap[record.Id].AddToContext(currentRequirementsSet);
                            }
                            else if (record.Method == "IfcRelDeclares")
                            {
                               // nothing to do, handled by LOIN library
                            }
                            else if (record.Method == "IfcPropertySetTemplate")
                            {

                                if (ifcPropertySetMap.ContainsKey(record.Id))
                                {
                                    PropertySetReused = true;
                                    currentPropertySet = ifcPropertySetMap[record.Id];
                                }
                                else
                                {
                                    PropertySetReused = false;
                                    currentPropertySet = model.CreatePropertySetTemplate(record.Par01, null); // "Základní informace o místnostech"
                                    //ApplicableEntity
                                    //Description
                                    if (record.Par02 != "")
                                        currentPropertySet.Description = record.Par02; // "Unikátní název místnosti. Pokud je budova rozdělena na bloky, je první písmeno označující blok budovy. Následující číslice obsahují podlaží a číslo místnosti."
                                                                                   //switch (record.Par03)
                                                                                   //{
                                                                                   //    case "IfcSpace":
                                                                                   //        Program.ifcPST.ApplicableEntity = nameof(IfcSpace);
                                                                                   //        break;
                                                                                   //    default:
                                                                                   //        Console.WriteLine("IfcPropertySetTemplate: UNKNOWN APPL ENT ",record.Par02);
                                                                                   //        Program.ifcPST.ApplicableEntity = record.Par02;
                                                                                   //        break;
                                                                                   //};
                                    if (record.GlobalId != "")
                                        currentPropertySet.GlobalId = record.GlobalId;
                                    ifcPropertySetMap.Add(record.Id, currentPropertySet);
                                };
                                currentRequirementsSet.Add(currentPropertySet);
                            }
                            else if (record.Method == "IfcSimplePropertyTemplate")
                            {
                                if (!PropertySetReused)
                                {
                                    if (!ifcPropertyMap.TryGetValue(record.Id, out IfcSimplePropertyTemplate propertyTemplate))
                                    {
                                        propertyTemplate = model.New<IfcSimplePropertyTemplate>(p =>
                                        {
                                            p.Name = record.Par01; // "Název"
                                        });
                                        if (record.GlobalId != "")
                                            propertyTemplate.GlobalId = record.GlobalId;
                                        //Description
                                        if (record.Par02 != "")
                                            propertyTemplate.Description = record.Par02; // "Unikátní název místnosti. Pokud je budova rozdělena na bloky, je první písmeno označující blok budovy. Následující číslice obsahují podlaží a číslo místnosti."
                                                                                       //PrimaryMeasureType
                                                                                       //switch (record.Par03)
                                                                                       //{
                                                                                       //    case "IfcIdentifier":
                                                                                       //        Program.ifcSPT.PrimaryMeasureType = nameof(IfcIdentifier);
                                                                                       //        break;
                                                                                       //    case "IfcAreaMeasure":
                                                                                       //        Program.ifcSPT.PrimaryMeasureType = nameof(IfcAreaMeasure);
                                                                                       //        break;
                                                                                       //    default:
                                                                                       //        Console.WriteLine("IfcSimplePropertyTemplate: UNKNOWN MEASURE TYPE ",record.Par03);
                                                                                       //        Program.ifcSPT.PrimaryMeasureType = record.Par03;
                                                                                       //        break;
                                                                                       //};
                                        if (record.Par03 != "") // dataunit
                                            propertyTemplate.PrimaryUnit = units[record.Par03];
                                        if (record.Par04 != "") // nameof(X) -> "X"
                                            propertyTemplate.PrimaryMeasureType = record.Par04;
                                        //TemplateType
                                        switch (record.Par05)
                                        {
                                            case "P_SINGLEVALUE":
                                                propertyTemplate.TemplateType = IfcSimplePropertyTemplateTypeEnum.P_SINGLEVALUE;
                                                break;
                                            case "P_ENUMERATEDVALUE":
                                                propertyTemplate.TemplateType = IfcSimplePropertyTemplateTypeEnum.P_ENUMERATEDVALUE;
                                                propertyTemplate.Enumerators = enums.GetOrAdd(record.Par06, record.Par07.Split(",", StringSplitOptions.RemoveEmptyEntries)); // { "Pondělí", "Úterý", "Středa", "Čtvrtek", "Pátek", "Sobota", "Neděle"}
                                                break;
                                            case "P_REFERENCEVALUE":
                                                propertyTemplate.TemplateType = IfcSimplePropertyTemplateTypeEnum.P_REFERENCEVALUE;
                                                break;
                                            case "P_BOUNDEDVALUE":
                                                propertyTemplate.TemplateType = IfcSimplePropertyTemplateTypeEnum.P_BOUNDEDVALUE;
                                                break;
                                            default:
                                                Console.WriteLine("IfcSimplePropertyTemplate: UNKNOWN TEMPLATE TYPE ", record.Par05);
                                                //Program.ifcSPT.TemplateType = ...
                                                break;
                                        };
                                        ifcPropertyMap.Add(record.Id, propertyTemplate);
                                    };

                                    currentPropertySet.HasPropertyTemplates.Add(propertyTemplate);
                                    currentPropertyTemplate = propertyTemplate;
                                };
                            }
                            else if (record.Method == "IfcSIUnit")
                            {
                                var unit = model.New<IfcSIUnit>();
                                //Name
                                //Program.ifcISU.Name = IfcSIUnitName.SQUARE_METRE;
                                try
                                {
                                    unit.Name = ifcSIUnitMap[record.Par01];
                                }
                                catch (KeyNotFoundException)
                                {
                                    Console.WriteLine("IfcSIUnit: UNKNOWN NAME ", record.Par01);
                                    //ifcISU.Name = IfcSIUnitName. ...
                                }
                                //UnitType
                                switch (record.Par02)
                                {
                                    case "AREAUNIT":
                                        unit.UnitType = IfcUnitEnum.AREAUNIT;
                                        break;
                                    default:
                                        Console.WriteLine("IfcSIUnit: UNKNOWN UNIT TYPE ", record.Par02);
                                        //ifcISU.UnitType = ...
                                        break;
                                };
                                currentPropertyTemplate.PrimaryUnit = unit;
                            }
                            else if (record.Method == "IfcDocumentReference")
                            {
                                // Declared data requirements / templates
                                model.New<IfcRelAssociatesDocument>(rd => {
                                    rd.RelatedObjects.Add(currentPropertyTemplate);
                                    rd.RelatingDocument = model.New<IfcDocumentReference>(doc => {
                                        doc.Identification = record.Par01; // "Vyhláška č. 441/2013 Sb."
                                        doc.Location = record.Par02; // "https://www.mfcr.cz/cs/legislativa/legislativni-dokumenty/2013/vyhlaska-c-441-2013-sb-16290"
                                        doc.Name = record.Par03; // "Vyhláška k provedení zákona o oceňování majetku (oceňovací vyhláška)"
                                    });
                                });
                            };
                        }; // konec while read
                        txn.Commit();
                    }

                    // validate schema and proper units
                    var validator = new IfcValidator();
                    var valid = validator.Check(model.Internal);
                    if (doValidate != "novalidate" && !valid)
                        throw new Exception("Invalid model shouldn't be stored and used for any purposes.");

                    // this is stdout if you want to use it
                    //var stdout = Console.OpenStandardOutput();

                    // writing to stream (any stream)
                    using (var stream = File.Create(ifcFile))
                    {
                        if (Path.GetExtension(ifcFile) == ".ifcxml")
                        {
                            model.Internal.SaveAsIfcXml(stream);
                        }
                        else
                        {
                            model.Internal.SaveAsIfc(stream);
                        };
                    }

                    //model.SaveAs("DDSS.ifc");
                    //model.SaveAs("DDSS.ifcxml");
                    //model.SaveAs(ifcFile); // args[0]
                }
                //STOP

            } // csv
        }
    }
}
