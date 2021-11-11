using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xbim.Ifc4.ExternalReferenceResource;

namespace LOIN.Server.Contracts
{
    public class GrouppedRequirementSets: LoinItem
    {
        public string NoteCS { get; set; }
        public string NoteEN { get; set; }

        public string IFCType { get; set; }
        public string IFCPredefinedType { get; set; }

        public string Code { get; set; }
        public List<int> Path { get; set; }
        public IEnumerable<NamedRequirementSet> RequirementSets { get; }

        public string CciSE { get; set; } // Stavebni entity   
        public string CciVS { get; set; } // Vybudovane systemy
        public string CciFS { get; set; } // Funkcni systemy   
        public string CciTS { get; set; } // Technicke systemy 
        public string CciKO { get; set; } // Komponenty        
        public string CciSK { get; set; } // Stavebni komplexy 

        public GrouppedRequirementSets(Context.BreakdownItem item, IEnumerable<NamedRequirementSet> requirementSets): base(item)
        {
            Code = item.Code;
            NoteCS = item.GetNote("cs");
            NoteEN = item.GetNote("en");

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
