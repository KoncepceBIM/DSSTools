using LOIN.Server.Exceptions;
using LOIN.Server.Swagger;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
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
        [EnableLoinContext]
        [ProducesResponseType(typeof(Contracts.RequirementSet[]), StatusCodes.Status200OK)]
        public IActionResult Get([FromQuery]bool expandContext = false)
        {
            try
            {
                var ctxMap = expandContext ? new Contracts.ContextMap(Model) : null;
                var ctx = expandContext ? BuildContext() : null;
                var loins = ApplyContextFilter();
                var requirementSets = loins.SelectMany(rs => rs.Requirements)
                    .Distinct()
                    .SelectMany(r => r.PartOfPsetTemplate).GroupBy(ps => ps.Name)
                    .Select(rs => new Contracts.RequirementSet(ctxMap, ctx, rs))
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
        public IActionResult GetSingle([FromRoute] int id, [FromQuery] bool expandContext = false)
        {
            try
            {
                var ctxMap = expandContext ? new Contracts.ContextMap(Model) : null;
                var ctx = expandContext ? BuildContext() : null;
                var result = Model.Internal.Instances[id] as IIfcPropertySetTemplate;
                return Ok(new Contracts.RequirementSet(ctxMap, ctx, result));
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
                var dto = pset.HasPropertyTemplates.Select(p => new Contracts.Requirement(p, pset));
                return Ok(dto);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
    }
}
