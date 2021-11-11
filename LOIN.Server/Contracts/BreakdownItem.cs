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
        public BreakdownItem(Context.BreakdownItem item, bool onlyWithRequirements, Func<BreakdownItem,string> orderBy): base(item)
        {
            Code = item.Code;

            NoteCS = item.GetNote("cs");
            NoteEN = item.GetNote("en");

            if (item.Children != null && item.Children.Any())
            {
                var query = onlyWithRequirements ?
                    item.Children.Where(c => c.HasRequirements) :
                    item.Children;
                Children = query.Select(c => new BreakdownItem(c, onlyWithRequirements, orderBy)).OrderBy(orderBy).ToList();
            }

            if (item.Entity is IfcClassificationReference cref)
            {
                IFCType = cref.GetIFCType();
                IFCPredefinedType = cref.GetIFCPredefinedType();

                CciSE = cref.GetCCI_SE("cs");
                CciVS = cref.GetCCI_VS("cs");
                CciFS = cref.GetCCI_FS("cs");
                CciTS = cref.GetCCI_TS("cs");
                CciKO = cref.GetCCI_KO("cs");
                CciSK = cref.GetCCI_SK("cs");
            }

        }
        public string Code { get; set; }

        public string NoteCS { get; set; }
        public string NoteEN { get; set; }

        public string IFCType { get; set; }
        public string IFCPredefinedType { get; set; }

        public string CciSE { get; set; } // Stavebni entity   
        public string CciVS { get; set; } // Vybudovane systemy
        public string CciFS { get; set; } // Funkcni systemy   
        public string CciTS { get; set; } // Technicke systemy 
        public string CciKO { get; set; } // Komponenty        
        public string CciSK { get; set; } // Stavebni komplexy 


        public List<BreakdownItem> Children { get; set; }
    }
}
