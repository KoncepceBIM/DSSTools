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
    public class ReasonsController : LoinController
    {
        public ReasonsController(ILogger<ReasonsController> logger) : base(logger)
        {
        }

        [HttpGet]
        [EnableQuery]
        [ProducesResponseType(typeof(Contracts.Reason[]), StatusCodes.Status200OK)]
        public IActionResult Get()
        {
            return Ok(Model.Reasons.Select(a => new Contracts.Reason(a)));
        }

    }
}
