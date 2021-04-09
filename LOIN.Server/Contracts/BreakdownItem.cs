using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOIN.Server.Contracts
{
    public class BreakdownItem: LoinItem
    {
        public BreakdownItem(Context.BreakdownItem item): base(item)
        {
            Code = item.Code;

            if (item.Children != null && item.Children.Any())
            {
                Children = item.Children.Select(c => new BreakdownItem(c)).ToList();
            }
        }
        public string Code { get; set; }
        public List<BreakdownItem> Children { get; set; }
    }
}
