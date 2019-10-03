using LOIN.Context;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;

namespace LOIN.Requirements
{
    public class RequirementsSet : AbstractLoinEntity<IfcProjectLibrary>
    {
        private readonly List<IfcRelDeclares> _relations;

        internal RequirementsSet(IfcProjectLibrary lib, Model model, List<IfcRelDeclares> relations): base(lib, model)
        {
            _relations = relations;
        }

        internal static IEnumerable<RequirementsSet> GetRequirements(Model model)
        {
            var cache = new Dictionary<IfcProjectLibrary, List<IfcRelDeclares>>();
            foreach (var lib in model.Internal.Instances.OfType<IfcProjectLibrary>())
                cache.Add(lib, new List<IfcRelDeclares>());
            foreach (var rel in model.Internal.Instances.OfType<IfcRelDeclares>())
            {
                if (!(rel.RelatingContext is IfcProjectLibrary l))
                    continue;
                cache[l].Add(rel);
            }
            return cache.Select(kvp => new RequirementsSet(kvp.Key, model, kvp.Value));
        }

        // context
        public IEnumerable<Actor> Actors => Model.Actors.Where(a => a.Contains(this));
        public IEnumerable<BreakedownItem> BreakedownItems => Model.BreakdownStructure.Where(a => a.Contains(this));
        public IEnumerable<Milestone> Milestones => Model.Milestones.Where(a => a.Contains(this));
        public IEnumerable<Reason> Reasons => Model.Reasons.Where(a => a.Contains(this));

        // requirements
        public IEnumerable<IfcPropertySetTemplate> Requirements => _relations.SelectMany(r => r.RelatedDefinitions.OfType<IfcPropertySetTemplate>());

        public void Remove(IfcPropertySetTemplate template)
        {
            foreach (var rel in _relations)
                rel.RelatedDefinitions.Remove(template);
        }

        public void Add(IfcPropertySetTemplate template)
        {
            if (!_relations.Any())
            {
                var rel = Model.Internal.Instances.New<IfcRelDeclares>(r => r.RelatingContext = Entity);
                _relations.Add(rel);
            }

            var relation = _relations.FirstOrDefault();
            relation.RelatedDefinitions.Add(template);
        }
    }
}
