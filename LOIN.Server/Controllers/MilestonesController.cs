using LOIN.Context;
using LOIN.Server.Services.Interfaces;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOIN.Server.Controllers
{
    [ApiController]
    [Route("{repositoryId}/[controller]")]
    public class MilestonesController : LoinController
    {
        public MilestonesController(ILogger<MilestonesController> logger) : base(logger)
        {
        }

        [HttpGet]
        [EnableQuery]
        [ProducesResponseType(typeof(Contracts.Milestone[]), StatusCodes.Status200OK)]
        public IActionResult Get()
        {
            return Ok(Model.Milestones.Select(a => new Contracts.Milestone(a)));
        }

    }
}
