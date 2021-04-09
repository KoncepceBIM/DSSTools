using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xbim.Ifc4.Interfaces;

namespace LOIN.Server.Contracts
{
    public class Requirement: LoinItem
    {
        public Requirement(IIfcPropertyTemplate property): base(property)
        {
            
        }
    }
}
