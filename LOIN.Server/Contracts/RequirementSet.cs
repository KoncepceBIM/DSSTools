using LOIN.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xbim.Ifc4.Interfaces;

namespace LOIN.Server.Contracts
{
    /// <summary>
    /// Set of LOIN requirements
    /// </summary>
    public class RequirementSet : LoinItem
    {
        /// <summary>
        /// Constructs the requirement set view based on the IIfcPropertySetTemplate
        /// </summary>
        /// <param name="template">IIfcPropertySetTemplate</param>
        internal RequirementSet(IIfcPropertySetTemplate template) : this(null, null, template)
        {
        }

        /// <summary>
        /// Constructs the requirement set view based on a set of IIfcPropertySetTemplate representing conceptually the same property set
        /// </summary>
        /// <param name="contextMap">Context map cache</param>
        /// <param name="filter">Current context filter</param>
        /// <param name="templates">List of templates</param>
        internal RequirementSet(ContextMap contextMap, IEnumerable<IContextEntity> filter, IEnumerable<IIfcPropertySetTemplate> templates) : base(templates.FirstOrDefault())
        {
            Requirements = templates.SelectMany(t => t.HasPropertyTemplates
                .Select(p => contextMap != null ? new Requirement(contextMap, filter, p, t) : new Requirement(p, t)))
                .ToList();
        }

        internal RequirementSet(ContextMap contextMap, IEnumerable<IContextEntity> filter, IIfcPropertySetTemplate template) : this(contextMap, filter, new[] { template})
        {

            
        }

        /// <summary>
        /// List of requirements
        /// </summary>
        public List<Requirement> Requirements { get; set; }
    }
}
