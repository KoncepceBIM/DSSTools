using LOIN.Context;
using System;
using System.Collections.Generic;
using System.Text;
using Xbim.Ifc4.Interfaces;

namespace LOIN.Requirements
{
    public class RequirementsSet : AbstractLoinEntity<IIfcProjectLibrary>
    {

        internal RequirementsSet(IIfcProjectLibrary lib, Model model): base(lib, model)
        {
        }

        // context
        public Actor Actor => throw new NotImplementedException();
        public BreakedownItem BreakedownItem => throw new NotImplementedException();
        public Milestone Milestone => throw new NotImplementedException();
        public Reason Reason => throw new NotImplementedException();

        // requirements
    }
}
