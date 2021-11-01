using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using LOIN.Requirements;
using Xbim.Common;
using Xbim.Ifc4.ExternalReferenceResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.MeasureResource;
using Xbim.Ifc4.PropertyResource;

namespace LOIN.Context
{
    public class BreakdownItem : AbstractLoinEntity<IfcClassificationSelect>, IContextEntity
    {
        public const string IfcMappingPsetName = "CZ_IFC_Mapping";
        public const string CCIMappingPsetName = "CZ_CCI_Mapping";

        private readonly List<IfcRelAssociatesClassification> _relations;
        private readonly HashSet<int> _cache;

        internal BreakdownItem(IfcClassificationSelect classification, Model model, List<IfcRelAssociatesClassification> relations) : base(classification, model)
        {
            _relations = relations;
            _cache = new HashSet<int>(_relations.SelectMany(r => r.RelatedObjects).Select(o => o.EntityLabel));
        }

        IPersistEntity IContextEntity.Entity => Entity;

        public string Id => Code;

        public BreakdownItem Parent { get; internal set; }

        private List<BreakdownItem> _children = new List<BreakdownItem>();
        public IEnumerable<BreakdownItem> Children => _children;

        public IEnumerable<BreakdownItem> Parents
        {
            get
            {
                var p = Parent;
                while (p != null)
                {
                    yield return p;
                    p = p.Parent;
                }
            }
        }

        internal void AddChild(BreakdownItem child)
        {
            _children.Add(child);
        }

        internal static IEnumerable<BreakdownItem> GetBreakdownStructure(Model model)
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
                .Select(kvp => new BreakdownItem(kvp.Key, model, kvp.Value))
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

                if (!lookUp.TryGetValue(parentEntity.EntityLabel, out BreakdownItem parentItem))
                {
                    throw new Exception("Unexpected type");
                }

                // build both directions
                item.Parent = parentItem;
                if (parentItem._children == null)
                    parentItem._children = new List<BreakdownItem>();
                parentItem._children.Add(item);
            }
            return allItems;
        }

        /// <summary>
        /// True if this breakdown item or any children has any requirements defined in their context
        /// </summary>
        public bool HasRequirements => _cache.Any() || (_children.Count > 0 && _children.Any(c => c.HasRequirements));

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

        public IEnumerable<IfcRelAssociatesClassification> Relations => _relations.AsReadOnly();

        public string Code
        {
            get => Entity is IfcClassificationReference r ? r.Identification?.ToString() : (Entity as IfcClassification)?.Edition?.ToString();
        }

        public string GetNote(string lang) => 
            Entity is IfcClassificationReference r ? r.GetNote(lang) : null;

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

        // public IfcPropertySet IfcMappingSet { get => GetPset(IfcMappingPsetName);  set => SetPset(value, IfcMappingPsetName); }
        // public IfcPropertySet CCIMappingSet { get => GetPset(CCIMappingPsetName);  set => SetPset(value, CCIMappingPsetName); }

        public CCIMapping GetOrCreateCCIMapping()
        {
            var pset = GetOrCreatePset(CCIMappingPsetName);
            return new CCIMapping(pset);
        }

        public IFCMapping GetOrCreateIFCMapping()
        {
            var pset = GetOrCreatePset(IfcMappingPsetName);
            return new IFCMapping(pset);
        }


        private IfcPropertySet GetOrCreatePset(string name)
        {
            var ps = GetPset(name);
            if (ps != null)
                return ps;
            ps = Model.Internal.Instances.New<IfcPropertySet>();
            SetPset(ps, name);
            return ps;
        }

        private IfcPropertySet GetPset(string name)
        {
            return _relations?
            .SelectMany(r => r.RelatedObjects)
            .OfType<IfcPropertySet>()
            .Where(ps => ps.Name == name)
            .FirstOrDefault();
        }

        private void SetPset(IfcPropertySet value, string name)
        {
            var rel = _relations
                   .Where(r => r.RelatedObjects.Any(o => o is IfcPropertySet ps && ps.Name == name))
                   .FirstOrDefault();
            if (rel == null && value == null)
                return;

            // remove current if exists
            if (rel != null)
            {
                var ps = rel.RelatedObjects
                    .OfType<IfcPropertySet>()
                    .Where(p => p.Name == name)
                    .FirstOrDefault();
                if (ps != null)
                    rel.RelatedObjects.Remove(ps);
            }

            // stop of no value is to be set
            if (value == null)
                return;

            // create if not exists
            if (rel == null)
            {
                rel = Model.Internal.Instances.New<IfcRelAssociatesClassification>(r =>
                {
                    r.GlobalId = Guid.NewGuid();
                    r.RelatingClassification = Entity;
                });
            }

            // add the value, make sure the name key is set
            value.Name = name;
            rel.RelatedObjects.Add(value);
        }
    }

    public abstract class MappingPropertySet
    {
        protected readonly IfcPropertySet set;

        protected MappingPropertySet(IfcPropertySet set)
        {
            this.set = set;
        }

        protected string Get([CallerMemberName] string name = null) => set.HasProperties.OfType<IfcPropertySingleValue>()
            .Where(p => p.Name == name).FirstOrDefault().NominalValue?.Value.ToString();
        protected void Set(string value, [CallerMemberName] string name = null)
        {
            var exists = set.HasProperties.OfType<IfcPropertySingleValue>()
            .Where(p => p.Name == name).FirstOrDefault();
            if (exists != null)
                exists.NominalValue = new IfcIdentifier(value);
            else
                set.HasProperties.Add(set.Model.Instances.New<IfcPropertySingleValue>(p => {
                    p.Name = name;
                    p.NominalValue = new IfcIdentifier(value);
                }));
        }
    }

    public class CCIMapping : MappingPropertySet
    {
        public CCIMapping(IfcPropertySet set) : base(set) { }

        public string StavebniKomplex { get => Get(); set=> Set(value); }
        public string StavebniEntita { get => Get(); set=> Set(value); }
        public string VybudovanyProstor { get => Get(); set=> Set(value); }
        public string FunkcniSystem { get => Get(); set=> Set(value); }
        public string KonstrukcniSystem { get => Get(); set=> Set(value); }
        public string Komponenta { get => Get(); set=> Set(value); }
    }

    public class IFCMapping : MappingPropertySet
    {
        public IFCMapping(IfcPropertySet set) : base(set) { }

        public string Entity { get => Get(); set => Set(value); }
        public string PredefinedType { get => Get(); set => Set(value); }

    }
}
