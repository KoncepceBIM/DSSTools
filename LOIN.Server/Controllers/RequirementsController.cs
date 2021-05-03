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
    [Route("api/{repositoryId}/[controller]")]
    public class RequirementsController : LoinController
    {
        public RequirementsController(ILogger<RequirementsController> logger) : base(logger)
        {
        }

        [HttpGet]
        [EnableQuery]
        [EnableLoinContext]
        [ProducesResponseType(typeof(Contracts.Requirement[]), StatusCodes.Status200OK)]
        public IActionResult Get()
        {
            try
            {
                var loins = ApplyContextFilter();
                var requirements = loins
                   .SelectMany(r => r.RequirementSets).Distinct()
                   .SelectMany(r => r.HasPropertyTemplates.Select(p => new Contracts.Requirement(p, r)));
                var directRequirements = loins
                   .SelectMany(r => r.DirectRequirements).Distinct()
                   .Select(r => new Contracts.Requirement(r, null));

                return Ok(requirements.Union(directRequirements));
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
                var parent = result.PartOfPsetTemplate.FirstOrDefault();
                return Ok(new Contracts.Requirement(result, parent));
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
    }
}
