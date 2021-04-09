using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOIN.Server.Contracts
{
    public class BreakdownItem: ContextItem
    {
        public BreakdownItem(Context.BreakdownItem item): base(item)
        {
            if (item.Children != null && item.Children.Any())
            {
                Children = item.Children.Select(c => new BreakdownItem(c)).ToList();
            }
        }

        public List<BreakdownItem> Children { get; set; }
    }
}
