using System;
using System.Collections.Generic;
using System.Linq;
using Xbim.Ifc4.ActorResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;

namespace LOIN.Context
{
    public class Actor : AbstractLoinEntity<IfcActor>, IContextEntity
    {
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
            return AddToContext(requirements, null as IfcActorRole);
        }

        public bool AddToContext(Requirements.RequirementsSet requirements, IfcRoleEnum role)
        {
            var actorRole = Entity.Model.Instances.FirstOrDefault<IfcActorRole>(r => r.Role == role);
            if (actorRole == null)
                actorRole = Model.New<IfcActorRole>(r => { r.Role = role; });
            
            return AddToContext(requirements, actorRole);
        }

        public bool AddToContext(Requirements.RequirementsSet requirements, string role)
        {
            var actorRole = Entity.Model.Instances.FirstOrDefault<IfcActorRole>(r => r.UserDefinedRole == role);
            if (actorRole == null)
                actorRole = Model.New<IfcActorRole>(r => { 
                    r.Role = IfcRoleEnum.USERDEFINED;
                    r.UserDefinedRole = role;
                });

            return AddToContext(requirements, actorRole);
        }

        public bool AddToContext(Requirements.RequirementsSet requirements, IfcActorRole role)
        {
            // it is there already
            if (IsContextFor(requirements))
                return false;

            var lib = requirements.Entity;
            var relations = role != null ? _relations.Where(r => r.ActingRole == role) : _relations;

            if (!relations.Any())
            {
                var rel = Model.Internal.Instances.New<IfcRelAssignsToActor>(r => {
                    r.RelatingActor = Entity;
                    r.ActingRole = role;
                });
                relations = new[] { rel };
                _relations.Add(rel);
            }

            var relation = relations.FirstOrDefault();
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

        public IEnumerable<IfcActorRole> GetRoles(Requirements.RequirementsSet requirements)
        {
            return _relations.Where(r => r.RelatedObjects.Contains(requirements.Entity)).Select(r => r.ActingRole);
        }
    }
}
