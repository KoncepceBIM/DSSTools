using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xbim.Ifc4.Interfaces;

namespace LOIN.Server.Contracts
{
    public class RequirementSet: LoinItem
    {
        public RequirementSet(IIfcPropertySetTemplate template): base(template)
        {
            if (template.HasPropertyTemplates.Count > 0)
            {
                Requirements = template.HasPropertyTemplates
                    .Select(p => new Requirement(p))
                    .ToList();
            }
        }

        public List<Requirement> Requirements { get; set; }
    }
}
