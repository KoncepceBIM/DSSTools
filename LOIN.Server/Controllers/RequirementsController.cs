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
    [Route("{repositoryId}/[controller]")]
    public class RequirementsController : LoinController
    {
        public RequirementsController(ILogger<RequirementsController> logger) : base(logger)
        {
        }

        [HttpGet]
        [EnableQuery]
        [ProducesResponseType(typeof(Contracts.Requirement[]), StatusCodes.Status200OK)]
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
                .SelectMany(r => r.HasPropertyTemplates)
                .Distinct()
                .Select(a => new Contracts.Requirement(a)));

            try
            {
                var context = BuildContext(
                    actors: actors,
                    reasons: reasons,
                    milestones: milestones,
                    breakdown: breakdown);

                var loins = ApplyContextFilter(context);
                var requirements = loins.SelectMany(rs => rs.Requirements).Distinct()
                    .Select(r => new Contracts.Requirement(r))
                    .ToList();

                return Ok(requirements);
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

        [HttpGet("{id}")]
        [EnableQuery]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Contracts.Requirement), StatusCodes.Status200OK)]
        public IActionResult GetSingle([FromRoute] int id)
        {
            try
            {
                var result = Model.Internal.Instances[id] as IIfcPropertyTemplate;
                return Ok(new Contracts.Requirement(result));
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
    }
}
