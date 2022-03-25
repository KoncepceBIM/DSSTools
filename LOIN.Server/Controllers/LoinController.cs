using LOIN.Context;
using LOIN.Server.Exceptions;
using LOIN.Server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xbim.Ifc4.Interfaces;

namespace LOIN.Server.Controllers
{
    public abstract class LoinController : ControllerBase
    {
        protected LoinController(ILogger logger)
        {
            Logger = logger;
        }

        protected ILogger Logger { get; }

        protected ILoinModel Model => HttpContext.Items[Constants.RepositoryContextKey] as ILoinModel;

        protected HashSet<IContextEntity> BuildContext()
        {
            var actors = GetFromRequest("actors");
            var reasons = GetFromRequest("reasons");
            var breakdown = GetFromRequest("breakdown");
            var milestones = GetFromRequest("milestones");

            return BuildContext(
                actors: actors.Count > 0 ? string.Join(",", actors) : null,
                reasons: reasons.Count > 0 ? string.Join(",", reasons) : null,
                milestones: milestones.Count > 0 ? string.Join(",", milestones) : null,
                breakdown: breakdown.Count > 0 ? string.Join(",", breakdown) : null
                );
        }

        protected HashSet<IContextEntity> BuildContext(string actors, string reasons, string milestones, string breakdown)
        {
            var ctx = new HashSet<IContextEntity>();

            foreach (var actor in GetContext(Model.Actors, actors))
                ctx.Add(actor);
            foreach (var reason in GetContext(Model.Reasons, reasons))
                ctx.Add(reason);
            foreach (var milestone in GetContext(Model.Milestones, milestones))
                ctx.Add(milestone);

            foreach (var item in GetContext(Model.BreakdownStructure, breakdown))
            {
                ctx.Add(item);

                if (item.Parent != null)
                {
                    foreach (var parent in item.Parents)
                    {
                        ctx.Add(parent);
                    }
                }
            }

            return ctx;
        }

        private StringValues GetFromRequest(string key)
        {
            if (HttpContext.Request.Query.TryGetValue(key, out StringValues values))
                return values;
            if (HttpContext.Request.HasFormContentType && HttpContext.Request.Form.TryGetValue(key, out values))
                return values;
            return StringValues.Empty;
        }

        private IEnumerable<T> GetContext<T>(IEnumerable<T> source, string ids) where T : IContextEntity
        {
            if (string.IsNullOrWhiteSpace(ids))
                return Enumerable.Empty<T>();

            var (labels, uuids )= ParseIds(ids);
            if (labels.Count == 0 && uuids.Count == 0)
                return Enumerable.Empty<T>();

            var result = source.Where(s => labels.Contains(s.Entity.EntityLabel) || uuids.Contains(s.Id)).ToList();
            if ((labels.Count + uuids.Count) > result.Count)
            {
                var notFoundLabels = labels.Except(result.Select(r => r.Entity.EntityLabel));
                var notFoundUUIDs = uuids.Except(result.Select(r => r.Id));
                var notFound = string.Join(", ", notFoundLabels.Select(l => l.ToString()).Concat(notFoundUUIDs).ToArray());
                throw new EntityNotFoundException(notFound, $"Context entity '{notFoundLabels}' doesn't exist");
            }


            return result;
        }


        private (HashSet<int>, HashSet<string>) ParseIds(string value)
        {
            var parts = value.Split(new char[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries);
            var ids = new HashSet<int>();
            var uuids = new HashSet<string>();
            foreach (var part in parts)
            {
                if (int.TryParse(part, out int intId))
                    ids.Add(intId);
                else
                    uuids.Add(part);
            }
            return (ids, uuids);
        }

        protected IEnumerable<Requirements.RequirementsSet> ApplyContextFilter()
        {
            var ctx = BuildContext();
            return ApplyContextFilter(ctx);
        }

        protected IEnumerable<Requirements.RequirementsSet> ApplyContextFilter(IEnumerable<IContextEntity> context)
        {
            var contextTypes = context.GroupBy(c => c.GetType());
            var loins = Model.Requirements;
            foreach (var contextType in contextTypes)
            {
                // continuous filtering refinement
                var lookUp = new HashSet<int>(contextType.SelectMany(c => c.RequirementsSetLookUp));
                loins = loins.Where(r => lookUp.Contains(r.Entity.EntityLabel));
            }

            return loins;
        }

        protected Func<IIfcPropertyTemplate, IIfcPropertySetTemplate, Contracts.Requirement> GetRequirementFactory(bool shouldExpandContexts)
        {
            if (shouldExpandContexts)
            {
                var map = Contracts.ContextMap.ForModel(Model);
                return (IIfcPropertyTemplate p, IIfcPropertySetTemplate ps) => new Contracts.Requirement(map, p, ps);
            }
            return (IIfcPropertyTemplate p, IIfcPropertySetTemplate ps) => new Contracts.Requirement(p, ps);
        }
    }
}
