using LOIN.Context;
using LOIN.Server.Exceptions;
using LOIN.Server.Swagger;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace LOIN.Server.Controllers
{
    [ApiController]
    [Route("api/{repositoryId}/[controller]")]
    public class BreakdownController : LoinController
    {
        public BreakdownController(ILogger<BreakdownController> logger) : base(logger)
        {
        }

        [HttpGet]
        [EnableQuery]
        [ProducesResponseType(typeof(Contracts.BreakdownItem[]), StatusCodes.Status200OK)]
        public IActionResult Get([FromQuery] bool nonEmpty = false)
        {
            return Ok(Model.BreakdownStructure.Where(i => i.Parent == null && i.HasRequirements)
                .Select(a => new Contracts.BreakdownItem(a, nonEmpty)));
        }

        [HttpGet("requirement-sets")]
        [EnableQuery]
        [EnableLoinContext]
        [ProducesResponseType(typeof(Contracts.GrouppedRequirementSets[]), StatusCodes.Status200OK)]
        public IActionResult GetGrouppedRequirementSets(GroupingType groupingType = GroupingType.IFC)
        {
            try
            {
                var ctx = BuildContext();
                var items = ctx.OfType<BreakdownItem>();
                if (!items.Any())
                    items = Model.BreakdownStructure;

                var loins = ApplyContextFilter(ctx);
                var result = new List<Contracts.GrouppedRequirementSets>();
                foreach (var item in items)
                {
                    var itemLoins = loins
                        .Where(rs => item.IsContextFor(rs)).ToList();
                    var requirements = itemLoins
                        .SelectMany(rs => rs.RequirementSets).Distinct()
                        .SelectMany(rs => rs.HasPropertyTemplates.Select(r => new Contracts.Requirement(r, rs)))
                        .Union(itemLoins.SelectMany(l => l.DirectRequirements)
                        .Distinct()
                        .Select(r => new Contracts.Requirement(r, r.PartOfPsetTemplate.FirstOrDefault())))
                        .ToList();
                    if (requirements.Any())
                    {
                        string getDescription(Contracts.Requirement r)
                        {
                            return groupingType switch
                            {
                                GroupingType.IFC => r.SetDescription,
                                GroupingType.EN => r.SetDescriptionEN,
                                GroupingType.CS => r.SetDescriptionCS,
                                _ => r.SetName
                            };
                        }

                        string getName(Contracts.Requirement r)
                        {
                            return groupingType switch
                            {
                                GroupingType.IFC => r.SetName,
                                GroupingType.EN => r.SetNameEN,
                                GroupingType.CS => r.SetNameCS,
                                _ => r.SetName
                            };
                        }

                        var requirementSets = requirements.GroupBy(getName)
                            .Select(g => new Contracts.NamedRequirementSet { 
                                Name = g.Key, 
                                Description = g.Select(getDescription).FirstOrDefault(d => !string.IsNullOrWhiteSpace(d)),
                                Requirements = g.OrderBy(getName)
                            });

                        result.Add(new Contracts.GrouppedRequirementSets(item, requirementSets));
                    }
                }

                return Ok(result);
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

        [HttpGet("requirements")]
        [EnableQuery]
        [EnableLoinContext]
        [ProducesResponseType(typeof(Contracts.GrouppedRequirements[]), StatusCodes.Status200OK)]
        public IActionResult GetGrouppedRequirements()
        {
            try
            {
                var ctx = BuildContext();
                var items = ctx.OfType<BreakdownItem>();
                if (!items.Any())
                    items = Model.BreakdownStructure;

                var loins = ApplyContextFilter(ctx);
                var result = new List<Contracts.GrouppedRequirements>();
                foreach (var item in items)
                {
                    var itemLoins = loins
                        .Where(rs => item.IsContextFor(rs)).ToList();
                    var requirementSets = itemLoins
                        .SelectMany(rs => rs.RequirementSets).Distinct()
                        .SelectMany(rs => rs.HasPropertyTemplates.Select(r => new Contracts.Requirement(r,rs)))
                        .Union(itemLoins.SelectMany(l => l.DirectRequirements)
                        .Distinct()
                        .Select(r => new Contracts.Requirement(r, r.PartOfPsetTemplate.FirstOrDefault())))
                        .ToList();
                    if (requirementSets.Any())
                        result.Add(new Contracts.GrouppedRequirements(
                            item, 
                            requirementSets.OrderBy(s => s.SetName).ThenBy(s => s.Name)
                            ));
                }

                return Ok(result);
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

    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum GroupingType
    {
        IFC,
        EN,
        CS
    }
}
