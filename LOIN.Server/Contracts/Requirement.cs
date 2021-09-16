using System;
using System.Collections.Generic;
using System.Linq;
using LOIN;
using Xbim.Ifc4.Interfaces;

namespace LOIN.Server.Contracts
{
    public class Requirement: LoinItem
    {
        private const string cs = "cs";
        private const string en = "en";

        public string Units { get; set; }
        public string ValueType { get; set; }
        public string DataType { get; set; }
        
        public string SetName { get; set; }
        public string SetNameCS { get; set; }
        public string SetNameEN { get; set; }

        public string SetDescription { get; set; }
        public string SetDescriptionCS { get; set; }
        public string SetDescriptionEN { get; set; }

        public List<string> Enumeration { get; set; }

        public List<string> Examples { get; set; }

        public Requirement(IIfcPropertyTemplate property, IIfcPropertySetTemplate set): base(property)
        {
            // set might be null, it the requirement is assigned directly, not through a set
            SetName = set?.Name;
            SetNameCS = set?.GetName(cs) ?? SetName;
            SetNameEN = set?.GetName(en) ?? SetName;

            SetDescription = set?.Description;
            SetDescriptionCS = set?.GetDescription(cs) ?? SetDescription;
            SetDescriptionEN = set?.GetDescription(en) ?? SetDescription;

            if (property is IIfcSimplePropertyTemplate simple)
            {
                DataType = simple.PrimaryMeasureType?.Value.ToString();
                ValueType = simple.TemplateType?.ToString();

                if (simple.PrimaryUnit != null)
                    Units = Unit.GetSymbol(simple.PrimaryUnit);
                else
                    Units = string.Empty;

                if (simple.Enumerators != null)
                {
                    Enumeration = simple.Enumerators.EnumerationValues.Select(v => v.Value.ToString()).ToList();
                }

                Examples = simple.GetExamples(set).Select(v => v.ToString()).ToList();
            }
            else
                throw new NotSupportedException("Only simple property templates are supported.");
        }
    }
}
