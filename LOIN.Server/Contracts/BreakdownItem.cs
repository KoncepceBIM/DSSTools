using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOIN.Server.Contracts
{
    public class BreakdownItem: LoinItem
    {
        public BreakdownItem(Context.BreakdownItem item, bool onlyWithRequirements): base(item)
        {
            Code = item.Code;

            if (item.Children != null && item.Children.Any())
            {
                var query = onlyWithRequirements ?
                    item.Children.Where(c => c.HasRequirements) :
                    item.Children;
                Children = query.Select(c => new BreakdownItem(c, onlyWithRequirements)).ToList();
            }
        }
        public string Code { get; set; }
        public List<BreakdownItem> Children { get; set; }
    }
}
