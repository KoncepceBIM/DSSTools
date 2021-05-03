using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xbim.Ifc4.Interfaces;

namespace LOIN.Server.Contracts
{
    public class Requirement: LoinItem
    {
        public string Units { get; set; }
        public string ValueType { get; set; }
        public string SetName { get; set; }

        public Requirement(IIfcPropertyTemplate property, IIfcPropertySetTemplate set): base(property)
        {
            // set might be null, it the requirement is assigned directly, not through a set
            SetName = set?.Name;

            if (property is IIfcSimplePropertyTemplate simple)
            {
                ValueType = simple.PrimaryMeasureType?.Value.ToString();

                if (simple.PrimaryUnit != null)
                    Units = Unit.GetSymbol(simple.PrimaryUnit);
                else
                    Units = string.Empty;
            }
            else
                throw new NotSupportedException("Only simple property templates are supported.");
        }
    }
}
