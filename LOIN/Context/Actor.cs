using System;
using System.Collections.Generic;
using System.Linq;
using Xbim.Ifc4.ActorResource;
using Xbim.Ifc4.Kernel;

namespace LOIN.Context
{
    public class Actor : AbstractLoinEntity<IfcActor>, IContextEntity
    {
        public bool Contains(Requirements.RequirementsSet requirements) => _cache.Contains(requirements.Entity.EntityLabel);

        public bool Remove(Requirements.RequirementsSet requirements)
        {
            var lib = requirements.Entity;
            // it is there already
            if (!Contains(requirements))
                return false;

            foreach (var rel in _relations)
                rel.RelatedObjects.Remove(lib);

            _cache.Remove(lib.EntityLabel);
            return true;
        }

        public bool Add(Requirements.RequirementsSet requirements)
        {
            // it is there already
            if (Contains(requirements))
                return false;

            var lib = requirements.Entity;

            if (!_relations.Any())
            {
                var rel = Model.Internal.Instances.New<IfcRelAssignsToActor>(r => r.RelatingActor = Entity);
                _relations.Add(rel);
            }

            var relation = _relations.FirstOrDefault();
            relation.RelatedObjects.Add(lib);
            _cache.Add(lib.EntityLabel);
            return true;
        }

        private readonly List<IfcRelAssignsToActor> _relations;
        private readonly HashSet<int> _cache = new HashSet<int>();

        internal Actor(IfcActor actor, Model model, List<IfcRelAssignsToActor> relations) : base(actor, model)
        {
            _relations = relations;
            _cache = new HashSet<int>(relations.SelectMany(r => r.RelatedObjects).Select(o => o.EntityLabel));
        }

        internal static IEnumerable<Actor> GetActors(Model model)
        {
            var cache = new Dictionary<IfcActor, List<IfcRelAssignsToActor>>();
            foreach (var actor in model.Internal.Instances.OfType<IfcActor>())
            {
                cache.Add(actor, new List<IfcRelAssignsToActor>());
            }
            foreach (var rel in model.Internal.Instances.OfType<IfcRelAssignsToActor>())
            {
                if (rel.RelatingActor == null)
                    continue;
                cache[rel.RelatingActor].Add(rel);
            }
            return cache.Select(kvp => new Actor(kvp.Key, model, kvp.Value));
        }

        public string Name
        {
            get
            {
                if (Entity.Name.HasValue)
                    return Entity.Name;
                if (Entity?.TheActor is IfcPerson person)
                    return $"{person.GivenName} {person.FamilyName}";
                if (Entity?.TheActor is IfcOrganization organization)
                    return organization.Name;
                if (Entity?.TheActor is IfcPersonAndOrganization pao)
                    return $"{pao.TheOrganization.Name}: {pao.ThePerson.GivenName} {pao.ThePerson.FamilyName}";
                return "";
            }
        }

        public string Description
        {
            get => Entity.Description;
            set => Entity.Description = value;
        }

        public IfcActorSelect PersonOrOrganization
        {
            get => Entity.TheActor;
            set => Entity.TheActor = value;
        }
    }
}
