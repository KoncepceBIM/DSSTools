using System;
using System.Collections.Generic;
using System.Text;
using Xbim.Ifc4.Interfaces;

namespace LOIN
{
    public class Actor : RelatedLoinEntity<IIfcRelAssignsToActor, IIfcActor>
    {
        public Actor(IIfcRelAssignsToActor relationship) : base(relationship)
        {
        }

        protected override Func<IIfcRelAssignsToActor, IIfcActor> Accessor => r => r.RelatingActor;
    }
}
