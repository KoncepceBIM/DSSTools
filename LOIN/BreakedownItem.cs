using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xbim.Ifc4.Interfaces;

namespace LOIN
{
    public class BreakedownItem: RelatedLoinEntity<IIfcRelAssociatesClassification, IIfcClassificationSelect>
    {
        public BreakedownItem(IIfcRelAssociatesClassification rel): base(rel)
        {

        }

        public BreakedownItem Parent { get; private set; }

        private List<BreakedownItem> _children;
        public IEnumerable<BreakedownItem> Children => _children;

        internal void CreateStructure(IEnumerable<BreakedownItem> allItems)
        {
            var lookUp = allItems.ToDictionary(i => i.Entity.EntityLabel);

            foreach (var item in allItems)
            {
                if (!(item.Entity is IIfcClassificationReference reference))
                    continue;

                if (reference.ReferencedSource == null)
                    continue;

                var parentEntity = reference.ReferencedSource;
                if (!lookUp.TryGetValue(parentEntity.EntityLabel, out BreakedownItem parentItem))
                {
                }

                // build both directions
                item.Parent = parentItem;
                if (parentItem._children == null)
                    parentItem._children = new List<BreakedownItem>();
                parentItem._children.Add(item);
            }
        }

        protected override Func<IIfcRelAssociatesClassification, IIfcClassificationSelect> Accessor => 
            r => r.RelatingClassification;

        public string Code
        {
            get =>  Entity is IIfcClassificationReference r ? r.Identification?.ToString() : (Entity as IIfcClassification)?.Edition?.ToString();
        }

        public string Uri
        {
            get =>  Entity is IIfcClassificationReference r ? r.Location?.ToString() : (Entity as IIfcClassification)?.Location?.ToString();
        }

        public string Name
        {
            get =>  Entity is IIfcClassificationReference r ? r.Name?.ToString() : (Entity as IIfcClassification)?.Name.ToString();
        }

        public string Description
        {
            get => Entity is IIfcClassificationReference r ? r.Description?.ToString() : (Entity as IIfcClassification)?.Description?.ToString();
        }

    }
}
