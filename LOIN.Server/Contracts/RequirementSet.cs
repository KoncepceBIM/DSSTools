using LOIN.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xbim.Ifc4.Interfaces;

namespace LOIN.Server.Contracts
{
    public class RequirementSet : LoinItem
    {
        public RequirementSet(IIfcPropertySetTemplate template) : this(null, null, template)
        {
        }

        public RequirementSet(ContextMap contextMap, IEnumerable<IContextEntity> filter, IEnumerable<IIfcPropertySetTemplate> templates) : base(templates.FirstOrDefault())
        {
            Requirements = templates.SelectMany(t => t.HasPropertyTemplates
                .Select(p => contextMap != null ? new Requirement(contextMap, filter, p, t) : new Requirement(p, t)))
                .ToList();
        }

        public RequirementSet(ContextMap contextMap, IEnumerable<IContextEntity> filter, IIfcPropertySetTemplate template) : this(contextMap, filter, new[] { template})
        {

            
        }

        public List<Requirement> Requirements { get; set; }
    }
}
