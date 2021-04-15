using LOIN.Server.Services.Interfaces;
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
    [Route("api/[controller]")]
    public class RepositoriesController : ControllerBase
    {
        private readonly ILogger<RepositoriesController> logger;
        private readonly ILoinRepository repositoryService;

        public RepositoriesController(ILogger<RepositoriesController> logger, ILoinRepository repositoryService)
        {
            this.logger = logger;
            this.repositoryService = repositoryService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Get()
        {
            var ids =  repositoryService.GetRepositoryIds().ToList();
            logger.LogDebug("Found {count} repositories");

            if (ids.Count == 0)
                return NotFound();

            return Ok(ids);
        }
    }
}
