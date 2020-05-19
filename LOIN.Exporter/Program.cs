using Serilog;
using System;
using System.IO;
using System.Collections.Generic;
using Xbim.Common;
using Xbim.Ifc;
using Xbim.Ifc4.ExternalReferenceResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.MeasureResource;

using CsvHelper;
using LOIN.Validation;
using LOIN.Context;
using LOIN.Requirements;

namespace LOIN.Exporter
{
    class RecordLine
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
        static Dictionary<string, BreakdownItem> breakedownRootMap;
        static IfcPropertySetTemplate currentPropertySet;
        static IfcSimplePropertyTemplate currentPropertyTemplate;
        //Console.WriteLine(ifcPL.GetType());

        //https://www.dotnetperls.com/map
        static Dictionary<string, IfcSIUnitName> ifcSIUnitMap;
        static Dictionary<int, BreakdownItem> breakedownMap; // mapa trid pouzitych v modelu
        static Dictionary<int, IfcSimplePropertyTemplate> ifcPropertyMap; // mapa vlastnosti pouzitych v modelu
        static Dictionary<int, IfcPropertySetTemplate> ifcPropertySetMap; // mapa skupin vlastnosti pouzitych v modelu
        static Dictionary<int, Reason> reasonsMap; // mapa IfcRelAssignsToControl pouzitych v modelu
        static Dictionary<string, Actor> actorMap; // mapa IfcRelAssignsToActor pouzitych v modelu
        static Dictionary<int, Milestone> milestonesCache; // mapa IfcRelAssignsToProcess pouzitych v modelu

        static bool PropertySetReused;

        static void Main(string[] args)
        {
            //
            ifcSIUnitMap = new Dictionary<string, IfcSIUnitName>
            {
                { "AMPERE", IfcSIUnitName.AMPERE },
                { "BECQUEREL", IfcSIUnitName.BECQUEREL },
                { "CANDELA", IfcSIUnitName.CANDELA },
                { "COULOMB", IfcSIUnitName.COULOMB },
                { "CUBIC_METRE", IfcSIUnitName.CUBIC_METRE },
                { "DEGREE_CELSIUS", IfcSIUnitName.DEGREE_CELSIUS },
                { "FARAD", IfcSIUnitName.FARAD },
                { "GRAM", IfcSIUnitName.GRAM },
                { "GRAY", IfcSIUnitName.GRAY },
                { "HENRY", IfcSIUnitName.HENRY },
                { "HERTZ", IfcSIUnitName.HERTZ },
                { "JOULE", IfcSIUnitName.JOULE },
                { "KELVIN", IfcSIUnitName.KELVIN },
                { "LUMEN", IfcSIUnitName.LUMEN },
                { "LUX", IfcSIUnitName.LUX },
                { "METRE", IfcSIUnitName.METRE },
                { "MOLE", IfcSIUnitName.MOLE },
                { "NEWTON", IfcSIUnitName.NEWTON },
                { "OHM", IfcSIUnitName.OHM },
                { "PASCAL", IfcSIUnitName.PASCAL },
                { "RADIAN", IfcSIUnitName.RADIAN },
                { "SECOND", IfcSIUnitName.SECOND },
                { "SIEMENS", IfcSIUnitName.SIEMENS },
                { "SIEVERT", IfcSIUnitName.SIEVERT },
                { "SQUARE_METRE", IfcSIUnitName.SQUARE_METRE },
                { "STERADIAN", IfcSIUnitName.STERADIAN },
                { "TESLA", IfcSIUnitName.TESLA },
                { "VOLT", IfcSIUnitName.VOLT },
                { "WATT", IfcSIUnitName.WATT },
                { "WEBER", IfcSIUnitName.WEBER }
            };
            //
            breakedownRootMap = new Dictionary<string, BreakdownItem>(); // prazdna mapa klasifikaci, bude plnena postupne
            //
            breakedownMap = new Dictionary<int, BreakdownItem>(); // prazdna mapa trid, bude plnena postupne
            ifcPropertyMap = new Dictionary<int, IfcSimplePropertyTemplate>(); // prazdna mapa vlastnosti, bude plnena postupne
            ifcPropertySetMap = new Dictionary<int, IfcPropertySetTemplate>(); // prazdna mapa skupin vlastnosti, bude plnena postupne
            reasonsMap = new Dictionary<int, Reason>(); // prazdna mapa IfcRelAssignsToControl, bude plnena postupne
            actorMap = new Dictionary<string, Actor>(); // prazdna mapa IfcRelAssignsToActor, bude plnena postupne
            milestonesCache = new Dictionary<int, Milestone>(); // prazdna mapa IfcRelAssignsToProcess, bude plnena postupne

            //using (var reader = new StreamReader("xBimFile.csv"))
            using (var reader = new StreamReader(Console.OpenStandardInput()))
            using (var csv = new CsvReader(reader))
            {

                var records = new List<RecordLine>();
                csv.Read(); // CSV header Line 0 - XbimEditorCredentials
                csv.ReadHeader();

                csv.Read(); // CSV line 0 - XbimEditorCredentials
                var record = new RecordLine
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
                            record = new RecordLine
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

                                // set name in default language
                                root.SetName("en", record.Par01);

                                // set name in other languages
                                root.SetName("cs", "");

                                breakedownRootMap.Add(record.Par01, root);
                            }
                            else if (record.Method == "IfcProjectLibrary")
                            {
                                currentRequirementsSet = model.CreateRequirementSet(record.Par01, null); // "Level of Information Need"
                                if (record.GlobalId != "")
                                    currentRequirementsSet.Entity.GlobalId = record.GlobalId;

                                // set name in default language
                                currentRequirementsSet.SetName("en", record.Par01);

                                // set name in other languages
                                currentRequirementsSet.SetName("cs", "");
                            }
                            else if (record.Method == "IfcRelAssignsToControl")
                            {
                                // Purpose of the data requirement/exchange
                                if (!reasonsMap.TryGetValue(record.Id, out Reason reason))
                                {
                                    reason = model.CreateReason(record.Par01, record.Par02);
                                    if (record.GlobalId != "")
                                        reason.Entity.GlobalId = record.GlobalId;
                                    reasonsMap.Add(record.Id, reason);

                                    // set name in default language
                                    reason.SetName("en", record.Par01);

                                    // set name in other languages
                                    reason.SetName("cs", "");
                                }
                                reason.AddToContext(currentRequirementsSet);
                            }
                            else if (record.Method == "IfcRelAssignsToActor")
                            {
                                // Actor / Role = Who is interested in the data
                                if (!actorMap.TryGetValue(record.GlobalId, out Actor actor))
                                {
                                    actor = model.CreateActor(record.Par01, null);
                                    if (record.GlobalId != "")
                                        actor.Entity.GlobalId = record.GlobalId;
                                    actorMap.Add(record.GlobalId, actor);

                                    // set name in default language
                                    actor.SetName("en", record.Par01);

                                    // set name in other languages
                                    actor.SetName("cs", "");
                                }
                                actor.AddToContext(currentRequirementsSet);
                            }
                            else if (record.Method == "IfcRelAssignsToProcess")
                            {
                                // Milestone = point in time
                                if (!milestonesCache.TryGetValue(record.Id, out Milestone milestone))
                                {
                                    milestone = model.CreateMilestone(record.Par01, null);
                                    milestone.Entity.IsMilestone = record.Par02 == "true";

                                    if (record.GlobalId != "")
                                        milestone.Entity.GlobalId = record.GlobalId;
                                    milestonesCache.Add(record.Id, milestone);

                                    // set name in default language
                                    milestone.SetName("en", record.Par01);

                                    // set name in other languages
                                    milestone.SetName("cs", "");
                                }
                                milestone.AddToContext(currentRequirementsSet);
                            }
                            else if (record.Method == "IfcClassificationReference")
                            {
                                // Class within classification
                                // Set parent
                                BreakdownItem parent = null;
                                if (record.Par03 == "")
                                    //Program.ifcCRS = Program.ifcCL; // classification for root class
                                    parent = breakedownRootMap[record.Par09]; // classification for root class
                                else
                                    parent = breakedownMap[record.Pid]; // parent class othewise
                                // Optionally add new class
                                if (!breakedownMap.ContainsKey(record.Id))
                                {
                                    var item = model.CreateBreakedownItem(record.Par01, record.Par02, null, parent);
                                    breakedownMap.Add(record.Id, item);

                                    // set name in default language
                                    item.SetName("en", record.Par01);

                                    // set name in other languages
                                    item.SetName("cs", "");
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

                                    // set name in default language
                                    currentPropertySet.SetName("en", record.Par01);

                                    // set name in other languages
                                    currentPropertySet.SetName("cs", "");

                                    //ApplicableEntity
                                    //Description
                                    if (record.Par02 != "")
                                    {
                                        currentPropertySet.Description = record.Par02;

                                        // set name in default language
                                        currentPropertySet.SetDescription("en", record.Par02);

                                        // set name in other languages
                                        currentPropertySet.SetDescription("cs", "");
                                    }
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

                                            // Set description in primary language
                                            p.SetName("en", record.Par01);

                                            // Set description in other languages
                                            p.SetName("cs", "");
                                        });
                                        if (record.GlobalId != "")
                                            propertyTemplate.GlobalId = record.GlobalId;
                                        //Description
                                        if (record.Par02 != "")
                                        { 
                                            propertyTemplate.Description = record.Par02;
                                            
                                            // Set description in primary language
                                            propertyTemplate.SetDescription("en", record.Par02);

                                            // Set description in other languages
                                            propertyTemplate.SetDescription("cs", "");
                                        }
                                        
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
