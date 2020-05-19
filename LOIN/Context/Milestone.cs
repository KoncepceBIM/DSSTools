using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xbim.Common;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.ProcessExtension;

namespace LOIN.Context
{
    public class Milestone : AbstractLoinEntity<IfcTask>, IContextEntity
    {
        private readonly List<IfcRelAssignsToProcess> _relations;
        private readonly HashSet<int> _cache;

        public string Id => Entity.GlobalId;

        public IEnumerable<IfcRelAssignsToProcess> Relations => _relations.AsReadOnly();


        internal Milestone(IfcTask task, Model model, List<IfcRelAssignsToProcess> relations) : base(task, model)
        {
            _relations = relations;
            _cache = new HashSet<int>(relations.SelectMany(r => r.RelatedObjects).Select(o => o.EntityLabel));
        }

        internal static IEnumerable<Milestone> GetMilestones(Model model)
        {
            var cache = new Dictionary<IfcTask, List<IfcRelAssignsToProcess>>();
            foreach (var actor in model.Internal.Instances.OfType<IfcTask>())
            {
                cache.Add(actor, new List<IfcRelAssignsToProcess>());
            }
            foreach (var rel in model.Internal.Instances.OfType<IfcRelAssignsToProcess>())
            {
                if (rel.RelatingProcess == null)
                    continue;
                cache[rel.RelatingProcess as IfcTask].Add(rel);
            }
            return cache.Select(kvp => new Milestone(kvp.Key, model, kvp.Value));
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

        public bool IsContextFor(Requirements.RequirementsSet requirements) => _cache.Contains(requirements.Entity.EntityLabel);

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
                var rel = Model.Internal.Instances.New<IfcRelAssignsToProcess>(r => r.RelatingProcess = Entity);
                _relations.Add(rel);
            }

            var relation = _relations.FirstOrDefault();
            relation.RelatedObjects.Add(lib);
            _cache.Add(lib.EntityLabel);
            return true;
        }
    }
}
