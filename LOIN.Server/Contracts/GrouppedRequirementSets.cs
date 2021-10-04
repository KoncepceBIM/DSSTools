using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOIN.Server.Contracts
{
    public class GrouppedRequirementSets: LoinItem
    {
        public string Code { get; set; }
        public List<int> Path { get; set; }
        public IEnumerable<NamedRequirementSet> RequirementSets { get; }

        public GrouppedRequirementSets(Context.BreakdownItem item, IEnumerable<NamedRequirementSet> requirementSets): base(item)
        {
            Code = item.Code;

            var path = new List<int> { item.Entity.EntityLabel };
            path.AddRange(item.Parents.Select(p => p.Entity.EntityLabel));
            path.Reverse();
            Path = path;
            RequirementSets = requirementSets;
        }
    }

    public class NamedRequirementSet
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public IEnumerable<Requirement> Requirements { get; set; }
    }
}
