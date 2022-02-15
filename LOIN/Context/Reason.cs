using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xbim.Common;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.SharedMgmtElements;

namespace LOIN.Context
{
    public class Reason : AbstractLoinEntity<IfcActionRequest>, IContextEntity
    {
        private readonly List<IfcRelAssignsToControl> _relations;
        private readonly HashSet<int> _cache;

        public string Id => Entity.GlobalId;
        IPersistEntity IContextEntity.Entity => Entity;

        public IEnumerable<IfcRelAssignsToControl> Relations => _relations.AsReadOnly();

        internal Reason(IfcActionRequest request, Model model, List<IfcRelAssignsToControl> relations) : base(request, model)
        {
            _relations = relations;
            _cache = new HashSet<int>(_relations.SelectMany(r => r.RelatedObjects).Select(o => o.EntityLabel));
        }

        internal static IEnumerable<Reason> GetReasons(Model model)
        {
            var cache = new Dictionary<IfcActionRequest, List<IfcRelAssignsToControl>>();
            foreach (var actor in model.Internal.Instances.OfType<IfcActionRequest>())
            {
                cache.Add(actor, new List<IfcRelAssignsToControl>());
            }
            foreach (var rel in model.Internal.Instances.OfType<IfcRelAssignsToControl>())
            {
                if (rel.RelatingControl == null)
                    continue;
                cache[rel.RelatingControl as IfcActionRequest].Add(rel);
            }
            return cache.Select(kvp => new Reason(kvp.Key, model, kvp.Value));
        }

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

        public bool IsContextFor(int requirementsLabel) => _cache.Contains(requirementsLabel);
        public bool IsContextFor(Requirements.RequirementsSet requirements) => _cache.Contains(requirements.Entity.EntityLabel);
        public HashSet<int> RequirementsSetLookUp => _cache;

        public bool RemoveFromContext(Requirements.RequirementsSet requirements)
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

        public bool AddToContext(Requirements.RequirementsSet requirements)
        {
            // it is there already
            if (IsContextFor(requirements))
                return false;

            var lib = requirements.Entity;

            if (!_relations.Any())
            {
                var rel = Model.Internal.Instances.New<IfcRelAssignsToControl>(r => r.RelatingControl = Entity);
                _relations.Add(rel);
            }

            var relation = _relations.FirstOrDefault();
            relation.RelatedObjects.Add(lib);
            _cache.Add(lib.EntityLabel);
            return true;
        }
    }
}
