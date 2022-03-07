using LOIN.Export;
using LOIN.Server.Equality;
using LOIN.Server.Exceptions;
using LOIN.Server.Swagger;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
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
        public IActionResult Get([FromQuery] bool expandContext = false)
        {
            try
            {
                var newRequirement = GetRequirementFactory(expandContext);
                var loins = ApplyContextFilter();
                var requirements = loins
                   .SelectMany(r => r.RequirementSets).Distinct()
                   .SelectMany(r => r.HasPropertyTemplates.Select(p => newRequirement(p, r)));
                var directRequirements = loins
                   .SelectMany(r => r.DirectRequirements).Distinct(PropertyEquality.Comparer)
                   .Select(r => newRequirement(r, r.PartOfPsetTemplate.FirstOrDefault()));

                var all = requirements
                    .Union(directRequirements, RequirementEquality.Comparer)
                    .Distinct(RequirementEquality.Comparer);

                return Ok(all);
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

        [HttpGet("export")]
        [EnableLoinContext]
        [FileResultContentType("application/octet-stream")]
        public IActionResult GetIFC()
        {
            return GetIFCFromForm(null);
        }

        [HttpPost("export")]
        [EnableLoinContext]
        [FileResultContentType("application/octet-stream")]
        public IActionResult GetIFCFromForm([FromForm]string fromUrl = null)
        {
            if (string.IsNullOrWhiteSpace(fromUrl))
                fromUrl = HttpContext.Request.GetDisplayUrl();
            try
            {
                var ctx = BuildContext();
                var result = IfcExporter.ExportContext(Model, ctx, fromUrl);
                var timestamp = DateTime.Now.ToString("s").Replace(" ", "_");
                return File(result, "application/octet-stream", timestamp + ".ifc");
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
        public IActionResult GetSingle([FromRoute] int id, [FromQuery] bool expandContext = false)
        {
            try
            {
                var newRequirement = GetRequirementFactory(expandContext);
                var result = Model.Internal.Instances[id] as IIfcPropertyTemplate;
                var parent = result.PartOfPsetTemplate.FirstOrDefault();
                return Ok(newRequirement(result, parent));
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

    }
}
