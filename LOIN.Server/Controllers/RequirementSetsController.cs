using LOIN.Server.Exceptions;
using LOIN.Server.Swagger;
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
        [EnableLoinContext]
        [ProducesResponseType(typeof(Contracts.RequirementSet[]), StatusCodes.Status200OK)]
        public IActionResult Get()
        {
            try
            {
                var loins = ApplyContextFilter();
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
