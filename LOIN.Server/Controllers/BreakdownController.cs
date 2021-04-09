using LOIN.Context;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace LOIN.Server.Controllers
{
    [ApiController]
    [Route("{repositoryId}/[controller]")]
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

    }
}
