using LOIN.Server.Exceptions;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xbim.Ifc4.Interfaces;

namespace LOIN.Server.Controllers
{
    [ApiController]
    [Route("api/{repositoryId}/requirement-sets")]
    public class RequirementSetsController : LoinController
    {
        public RequirementSetsController(ILogger<RequirementSetsController> logger) : base(logger)
        {
        }

        [HttpGet]
        [EnableQuery]
        [ProducesResponseType(typeof(Contracts.RequirementSet[]), StatusCodes.Status200OK)]
        public IActionResult Get(
            [FromQuery(Name = "actors")] string actors = null,
            [FromQuery(Name = "reasons")] string reasons = null,
            [FromQuery(Name = "breakdown")] string breakdown = null,
            [FromQuery(Name = "milestones")] string milestones = null)
        {

            if (
                string.IsNullOrWhiteSpace(actors) &&
                string.IsNullOrWhiteSpace(reasons) &&
                string.IsNullOrWhiteSpace(breakdown) &&
                string.IsNullOrWhiteSpace(milestones)
                )
                return Ok(Model.Requirements
                    .SelectMany(r => r.RequirementSets)
                    .Distinct()
                    .Select(a => new Contracts.RequirementSet(a)));

            try
            {
                var context = BuildContext(
                    actors: actors,
                    reasons: reasons,
                    milestones: milestones,
                    breakdown: breakdown);

                var loins = ApplyContextFilter(context);

                var requirementSets = loins.SelectMany(rs => rs.RequirementSets).Distinct()
                    .Select(rs => new Contracts.RequirementSet(rs))
                    .ToList();
                return Ok(requirementSets);
            }
            catch (EntityNotFoundException e)
            {
                return NotFound(new ProblemDetails { 
                    Detail = e.Message,
                    Status  = StatusCodes.Status404NotFound,
                    Type = nameof(EntityNotFoundException),
                    Title = $"Context entity '{e.EntityLabel}' not found"
                });
            }
        }

        [HttpGet("{id}")]
        [EnableQuery]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Contracts.RequirementSet), StatusCodes.Status200OK)]
        public IActionResult GetSingle([FromRoute] int id)
        {
            try
            {
                var result = Model.Internal.Instances[id] as IIfcPropertySetTemplate;
                return Ok(new Contracts.RequirementSet(result));
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [HttpGet("{id}/requirements")]
        [EnableQuery]
        [ProducesResponseType(typeof(Contracts.Requirement[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetRequirements([FromRoute] int id)
        {
            try
            {
                var pset = Model.Internal.Instances[id] as IIfcPropertySetTemplate;
                var dto = pset.HasPropertyTemplates.Select(p => new Contracts.Requirement(p));
                return Ok(dto);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
    }
}
