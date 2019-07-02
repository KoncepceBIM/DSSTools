using System;
using System.Collections.Generic;
using System.Text;
using Xbim.Ifc4.Interfaces;

namespace LOIN
{
    public class BreakedownItem: RelatedLoinEntity<IIfcRelAssociatesClassification, IIfcClassificationReference>
    {
        public BreakedownItem(IIfcRelAssociatesClassification rel): base(rel)
        {

        }

        protected override Func<IIfcRelAssociatesClassification, IIfcClassificationReference> Accessor => 
            r => r.RelatingClassification as IIfcClassificationReference;

        public string Code
        {
            get => Entity.Identification;
            set => Entity.Identification = value;
        }

        public string Uri
        {
            get => Entity.Location;
            set => Entity.Location = value;
        }

        public string Name
        {
            get => Entity.Name;
            set => Entity.Name = value;
        }

    }
}
