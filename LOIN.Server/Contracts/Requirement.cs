using System;
using System.Collections.Generic;
using System.Linq;
using Xbim.Ifc4.Interfaces;

namespace LOIN.Server.Contracts
{
    public class Requirement: LoinItem
    {
        private const string lang = "cs";

        public string Units { get; set; }
        public string ValueType { get; set; }
        public string DataType { get; set; }
        public string SetName { get; set; }
        public string SetIdentifier { get; set; }

        public List<string> Enumeration { get; set; }

        public Requirement(IIfcPropertyTemplate property, IIfcPropertySetTemplate set): base(property)
        {
            // set might be null, it the requirement is assigned directly, not through a set
            SetName = set?.Name;
            SetIdentifier = set.GetName(lang) ?? SetName;

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
            }
            else
                throw new NotSupportedException("Only simple property templates are supported.");
        }
    }
}
