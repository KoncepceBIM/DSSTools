using System;
using System.Collections.Generic;
using System.Text;
using Xbim.Ifc4.Interfaces;

namespace LOIN
{
    public class Reason : RelatedLoinEntity<IIfcRelAssignsToControl, IIfcActionRequest>
    {
        public Reason(IIfcRelAssignsToControl relationship) : base(relationship)
        {
        }

        protected override Func<IIfcRelAssignsToControl, IIfcActionRequest> Accessor =>
            r => r.RelatingControl as IIfcActionRequest;

        public string Name
        {
            get => Entity.Name;
            set => Entity.Name = value;
        }

        public string Description
        {
            get => Entity.Description;
            set => Entity.Description = value;
        }
    }
}
