using LOIN.Context;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xbim.Ifc4.ExternalReferenceResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;

namespace LOIN.Requirements
{
    public class RequirementsSet : AbstractLoinEntity<IfcProjectLibrary>
    {
        private readonly List<IfcRelDeclares> _relations;
        private readonly HashSet<GeometryRequirements> _geomRequirements;
     
        public IEnumerable<IfcRelDeclares> Relations => _relations.AsReadOnly();

        internal RequirementsSet(IfcProjectLibrary lib, Model model, List<IfcRelDeclares> relations): base(lib, model)
        {
            _relations = relations;

            // keep cache
            _geomRequirements = new HashSet<GeometryRequirements>(_relations
                .SelectMany(r => r.RelatedDefinitions.Where<IfcPropertySet>(ps => ps.Name == GeometryRequirements.PSetName))
                .Select(ps => new GeometryRequirements(ps)));
        }

        internal static IEnumerable<RequirementsSet> GetRequirements(Model model)
        {
            var cache = new Dictionary<IfcProjectLibrary, List<IfcRelDeclares>>();
            foreach (var lib in model.Internal.Instances.OfType<IfcProjectLibrary>())
                cache.Add(lib, new List<IfcRelDeclares>());
            foreach (var rel in model.Internal.Instances.OfType<IfcRelDeclares>())
            {
                if (!(rel.RelatingContext is IfcProjectLibrary lib))
                    continue;
                cache[lib].Add(rel);
            }
            return cache.Select(kvp => new RequirementsSet(kvp.Key, model, kvp.Value));
        }

        // context
        public IEnumerable<Actor> Actors => Model.Actors.Where(a => a.IsContextFor(this));
        public IEnumerable<BreakdownItem> BreakedownItems => Model.BreakdownStructure.Where(a => a.IsContextFor(this));
        public IEnumerable<Milestone> Milestones => Model.Milestones.Where(a => a.IsContextFor(this));
        public IEnumerable<Reason> Reasons => Model.Reasons.Where(a => a.IsContextFor(this));

        // alphanumeric and document requirements which are grouped in sets
        public IEnumerable<IfcPropertySetTemplate> RequirementSets => _relations.SelectMany(r => r.RelatedDefinitions.OfType<IfcPropertySetTemplate>());

        // alphanumeric and document requirements which are grouped in sets or are referenced directly
        public IEnumerable<IfcPropertyTemplate> Requirements => _relations.SelectMany(r => r.RelatedDefinitions.OfType<IfcPropertyTemplate>())
            .Union(RequirementSets.SelectMany(r => r.HasPropertyTemplates));

        /// <summary>
        /// requirements assigned to the requirement set directly, not through any property set template
        /// </summary>
        public IEnumerable<IfcPropertyTemplate> DirectRequirements => _relations.SelectMany(r => r.RelatedDefinitions.OfType<IfcPropertyTemplate>());


        // alphanumeric requirements which are grouped in sets or are referenced directly
        public IEnumerable<IfcPropertyTemplate> AlphanumericRequirements => _relations.SelectMany(r => r.RelatedDefinitions.OfType<IfcPropertyTemplate>())
            .Union(RequirementSets.SelectMany(r => r.HasPropertyTemplates))
            .Where(p =>
                !(p is IfcSimplePropertyTemplate sp &&
                sp.TemplateType == IfcSimplePropertyTemplateTypeEnum.P_REFERENCEVALUE &&
                sp.PrimaryMeasureType == nameof(IfcDocumentReference)));

        // document requirements which are grouped in sets or are referenced directly
        public IEnumerable<IfcPropertyTemplate> DocumentRequirements => _relations.SelectMany(r => r.RelatedDefinitions.OfType<IfcPropertyTemplate>())
            .Union(RequirementSets.SelectMany(r => r.HasPropertyTemplates))
            .Where(p => 
                p is IfcSimplePropertyTemplate sp && 
                sp.TemplateType == IfcSimplePropertyTemplateTypeEnum.P_REFERENCEVALUE &&
                sp.PrimaryMeasureType == nameof(IfcDocumentReference));

        // geometry requirements
        public IEnumerable<GeometryRequirements> GeomRequirements => _geomRequirements
            .ToList().AsReadOnly();

        public void Remove(IfcPropertyTemplateDefinition template)
        {
            foreach (var rel in _relations)
                rel.RelatedDefinitions.Remove(template);
        }

        public void Remove(GeometryRequirements geometryRequirements)
        {
            foreach (var rel in _relations)
                rel.RelatedDefinitions.Remove(geometryRequirements._pSet);
            _geomRequirements.Remove(geometryRequirements);
        }

        public void Add(IfcPropertyTemplateDefinition template)
        {
            if (!_relations.Any())
            {
                var rel = Model.Internal.Instances.New<IfcRelDeclares>(r => r.RelatingContext = Entity);
                _relations.Add(rel);
            }

            var relation = _relations.FirstOrDefault();
            relation.RelatedDefinitions.Add(template);
        }

        public void Add(GeometryRequirements geometryRequirements)
        {
            if (!_relations.Any())
            {
                var rel = Model.Internal.Instances.New<IfcRelDeclares>(r => r.RelatingContext = Entity);
                _relations.Add(rel);
            }

            var relation = _relations.FirstOrDefault();
            relation.RelatedDefinitions.Add(geometryRequirements._pSet);
            _geomRequirements.Add(geometryRequirements);
        }
    }
}
