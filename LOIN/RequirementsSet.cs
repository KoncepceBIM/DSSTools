using System;
using System.Collections.Generic;
using System.Text;
using Xbim.Ifc4.Interfaces;

namespace LOIN
{
    class RequirementsSet : AbstractLoinEntity<IIfcProjectLibrary>
    {
        private readonly IIfcProjectLibrary lib;

        public RequirementsSet(IIfcProjectLibrary lib)
        {
            this.lib = lib;
        }

        public override IIfcProjectLibrary Entity => lib;

        // context
        public Actor Actor => throw new NotImplementedException();
        public BreakedownItem BreakedownItem => throw new NotImplementedException();
        public Milestone Milestone => throw new NotImplementedException();
        public Reason Reason => throw new NotImplementedException();

        // requirements
    }
}
