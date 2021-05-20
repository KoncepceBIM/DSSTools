using LOIN.Context;
using LOIN.Server.Exceptions;
using LOIN.Server.Swagger;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace LOIN.Server.Controllers
{
    [ApiController]
    [Route("api/{repositoryId}/[controller]")]
    public class BreakdownController : LoinController
    {
        public BreakdownController(ILogger<BreakdownController> logger) : base(logger)
        {
        }

        [HttpGet]
        [EnableQuery]
        [ProducesResponseType(typeof(Contracts.BreakdownItem[]), StatusCodes.Status200OK)]
        public IActionResult Get()
        {
            return Ok(Model.BreakdownStructure.Where(i => i.Parent == null)
                .Select(a => new Contracts.BreakdownItem(a)));
        }

        [HttpGet("requirement-sets")]
        [EnableQuery]
        [EnableLoinContext]
        [ProducesResponseType(typeof(Contracts.GrouppedRequirementSets[]), StatusCodes.Status200OK)]
        public IActionResult GetGrouppedRequirementSets()
        {
            try
            {
                var ctx = BuildContext();
                var items = ctx.OfType<Context.BreakdownItem>();
                if (!items.Any())
                    items = Model.BreakdownStructure;

                var loins = ApplyContextFilter(ctx);
                var result = new List<Contracts.GrouppedRequirementSets>();
                foreach (var item in items)
                {
                    var requirementSets = loins
                        .Where(rs => item.IsContextFor(rs))
                        .SelectMany(rs => rs.RequirementSets).Distinct()
                        .Select(rs => new Contracts.RequirementSet(rs))
                        .ToList();
                    if (requirementSets.Any())
                        result.Add(new Contracts.GrouppedRequirementSets(
                            item, 
                            requirementSets.OrderBy(s => s.Name)
                            ));
                }

                return Ok(result);
            }
            catch (EntityNotFoundException e)
            {
                return NotFound(new ProblemDetails
                {
                    Detail = e.Message,
                    Status = StatusCodes.Status404NotFound,
                    Type = nameof(EntityNotFoundException),
                    Title = $"Context entity '{e.EntityLabel}' not found"
                });
            }
        }

        [HttpGet("requirements")]
        [EnableQuery]
        [EnableLoinContext]
        [ProducesResponseType(typeof(Contracts.GrouppedRequirements[]), StatusCodes.Status200OK)]
        public IActionResult GetGrouppedRequirements()
        {
            try
            {
                var ctx = BuildContext();
                var items = ctx.OfType<BreakdownItem>();
                if (!items.Any())
                    items = Model.BreakdownStructure;

                var loins = ApplyContextFilter(ctx);
                var result = new List<Contracts.GrouppedRequirements>();
                foreach (var item in items)
                {
                    var itemLoins = loins
                        .Where(rs => item.IsContextFor(rs)).ToList();
                    var requirementSets = itemLoins
                        .SelectMany(rs => rs.RequirementSets).Distinct()
                        .SelectMany(rs => rs.HasPropertyTemplates.Select(r => new Contracts.Requirement(r,rs)))
                        .Union(itemLoins.SelectMany(l => l.DirectRequirements).Distinct().Select(r => new Contracts.Requirement(r, null)))
                        .ToList();
                    if (requirementSets.Any())
                        result.Add(new Contracts.GrouppedRequirements(
                            item, 
                            requirementSets.OrderBy(s => s.SetName).ThenBy(s => s.Name)
                            ));
                }

                return Ok(result);
            }
            catch (EntityNotFoundException e)
            {
                return NotFound(new ProblemDetails
                {
                    Detail = e.Message,
                    Status = StatusCodes.Status404NotFound,
                    Type = nameof(EntityNotFoundException),
                    Title = $"Context entity '{e.EntityLabel}' not found"
                });
            }
        }

    }
}
