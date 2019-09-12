using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xbim.Common;
using Xbim.Ifc4.Interfaces;

namespace LOIN.Context
{
    public class BreakedownItem : AbstractLoinEntity<IIfcClassificationSelect>
    {
        public static readonly BreakedownItem Any = new BreakedownItem(null, null);

        internal BreakedownItem(IIfcClassificationSelect classification, Model model) : base(classification, model)
        {

        }

        public BreakedownItem Parent { get; private set; }

        private List<BreakedownItem> _children = new List<BreakedownItem>();
        public IEnumerable<BreakedownItem> Children => _children;

        internal static IEnumerable<BreakedownItem> CreateBreakdownStructure(Model model)
        {
            var allItems = model.IfcModel.Instances
                .OfType<IIfcClassificationSelect>()
                .Select(c => new BreakedownItem(c, model))
                .ToList();
            var lookUp = allItems.ToDictionary(i => i.Entity.EntityLabel);
            foreach (var item in allItems)
            {
                if (!(item.Entity is IIfcClassificationReference reference))
                    continue;

                var parentEntity = reference.ReferencedSource;
                // root entity
                if (parentEntity == null)
                    continue;

                if (!lookUp.TryGetValue(parentEntity.EntityLabel, out BreakedownItem parentItem))
                {
                    throw new Exception("Unexpected type");
                }

                // build both directions
                item.Parent = parentItem;
                if (parentItem._children == null)
                    parentItem._children = new List<BreakedownItem>();
                parentItem._children.Add(item);
            }
            return allItems;
        }


        public string Code
        {
            get => Entity is IIfcClassificationReference r ? r.Identification?.ToString() : (Entity as IIfcClassification)?.Edition?.ToString();
        }

        public string Uri
        {
            get => Entity is IIfcClassificationReference r ? r.Location?.ToString() : (Entity as IIfcClassification)?.Location?.ToString();
        }

        public string Name
        {
            get => Entity is IIfcClassificationReference r ? r.Name?.ToString() : (Entity as IIfcClassification)?.Name.ToString();
        }

        public string Description
        {
            get => Entity is IIfcClassificationReference r ? r.Description?.ToString() : (Entity as IIfcClassification)?.Description?.ToString();
        }

    }
}
