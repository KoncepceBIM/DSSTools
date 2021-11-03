using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOIN;
using Xbim.Ifc4.ExternalReferenceResource;
using Xbim.Ifc4.Interfaces;

namespace LOIN.Server.Contracts
{
    public class BreakdownItem: LoinItem
    {
        public BreakdownItem(Context.BreakdownItem item, bool onlyWithRequirements): base(item)
        {
            Code = item.Code;

            NoteCS = item.GetNote("cs");
            NoteEN = item.GetNote("en");

            if (item.Children != null && item.Children.Any())
            {
                var query = onlyWithRequirements ?
                    item.Children.Where(c => c.HasRequirements) :
                    item.Children;
                Children = query.Select(c => new BreakdownItem(c, onlyWithRequirements)).ToList();
            }

            if (item.Entity is IfcClassificationReference cref)
            {
                IFCType = cref.GetIFCType();
                IFCPredefinedType = cref.GetIFCPredefinedType();
            }

        }
        public string Code { get; set; }

        public string NoteCS { get; set; }
        public string NoteEN { get; set; }

        public string IFCType { get; set; }
        public string IFCPredefinedType { get; set; }

        public List<BreakdownItem> Children { get; set; }
    }
}
