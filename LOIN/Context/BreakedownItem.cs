using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LOIN.Requirements;
using Xbim.Common;
using Xbim.Ifc4.ExternalReferenceResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;

namespace LOIN.Context
{
    public class BreakedownItem : AbstractLoinEntity<IfcClassificationSelect>, IContextEntity
    {
        private readonly List<IfcRelAssociatesClassification> _relations;
        private readonly HashSet<int> _cache;

        internal BreakedownItem(IfcClassificationSelect classification, Model model, List<IfcRelAssociatesClassification> relations) : base(classification, model)
        {
            _relations = relations;
            _cache = new HashSet<int>(_relations.SelectMany(r => r.RelatedObjects).Select(o => o.EntityLabel));
        }

        public BreakedownItem Parent { get; internal set; }

        private List<BreakedownItem> _children = new List<BreakedownItem>();
        public IEnumerable<BreakedownItem> Children => _children;

        internal void AddChild(BreakedownItem child)
        {
            _children.Add(child);
        }

        internal static IEnumerable<BreakedownItem> GetBreakdownStructure(Model model)
        {
            var cache = new Dictionary<IfcClassificationSelect, List<IfcRelAssociatesClassification>>();
            foreach (var cls in model.Internal.Instances.OfType<IfcClassificationSelect>())
                cache.Add(cls, new List<IfcRelAssociatesClassification>());
            foreach (var rel in model.Internal.Instances.OfType<IfcRelAssociatesClassification>())
            {
                if (rel.RelatingClassification == null)
                    continue;
                cache[rel.RelatingClassification].Add(rel);
            }

            var allItems = cache
                .Select(kvp => new BreakedownItem(kvp.Key, model, kvp.Value))
                .ToList();
            var lookUp = allItems.ToDictionary(i => i.Entity.EntityLabel);
            foreach (var item in allItems)
            {
                if (!(item.Entity is IfcClassificationReference reference))
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

        public bool IsContextFor(RequirementsSet requirements) => _cache.Contains(requirements.Entity.EntityLabel);

        public bool RemoveFromContext(RequirementsSet requirements)
        {
            var lib = requirements.Entity;
            // it is there already
            if (!IsContextFor(requirements))
                return false;

            foreach (var rel in _relations)
                rel.RelatedObjects.Remove(lib);

            _cache.Remove(lib.EntityLabel);
            return true;
        }

        public bool AddToContext(RequirementsSet requirements)
        {
            // it is there already
            if (IsContextFor(requirements))
                return false;

            var lib = requirements.Entity;

            if (!_relations.Any())
            {
                var rel = Model.Internal.Instances.New<IfcRelAssociatesClassification>(r => r.RelatingClassification = Entity);
                _relations.Add(rel);
            }

            var relation = _relations.FirstOrDefault();
            relation.RelatedObjects.Add(lib);
            _cache.Add(lib.EntityLabel);
            return true;
        }

        public IEnumerable<IfcRelAssociatesClassification>  Relations => _relations.AsReadOnly();

        public string Code
        {
            get => Entity is IfcClassificationReference r ? r.Identification?.ToString() : (Entity as IfcClassification)?.Edition?.ToString();
        }

        public string Uri
        {
            get => Entity is IfcClassificationReference r ? r.Location?.ToString() : (Entity as IfcClassification)?.Location?.ToString();
        }

        public string Name
        {
            get => Entity is IfcClassificationReference r ? r.Name?.ToString() : (Entity as IfcClassification)?.Name.ToString();
        }

        public string Description
        {
            get => Entity is IfcClassificationReference r ? r.Description?.ToString() : (Entity as IfcClassification)?.Description?.ToString();
        }

    }
}
