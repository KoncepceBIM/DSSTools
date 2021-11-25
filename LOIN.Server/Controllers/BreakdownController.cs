using LOIN.Context;
using LOIN.Server.Equality;
using LOIN.Server.Exceptions;
using LOIN.Server.Swagger;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using Xbim.Ifc4.Kernel;

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
        [ProducesResponseType(typeof(Contracts.BreakdownItem[]), StatusCodes.Status200OK)]
        public IActionResult Get([FromQuery] bool nonEmpty = false, [FromQuery] string orderBy = nameof(Contracts.BreakdownItem.NameCS))
        {
            var type = typeof(Contracts.BreakdownItem);
            var orderInfo = type.GetProperty(nameof(Contracts.BreakdownItem.Name));
            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                var found = type.GetProperty(orderBy, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);
                if (found != null)
                    orderInfo = found;
            }

            string order(Contracts.BreakdownItem item)
            {
                return orderInfo.GetValue(item)?.ToString() ?? "";
            }

            return Ok(Model.BreakdownStructure.Where(i => i.Parent == null && i.HasRequirements)
                .Select(a => new Contracts.BreakdownItem(a, nonEmpty, order)));
        }

        [HttpGet("requirement-sets")]
        [EnableQuery]
        [EnableLoinContext]
        [ProducesResponseType(typeof(Contracts.GrouppedRequirementSets[]), StatusCodes.Status200OK)]
        public IActionResult GetGrouppedRequirementSets(GroupingType groupingType = GroupingType.CS)
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
                        .Distinct(PropertyEquality.Comparer)
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

                        string getSetName(Contracts.Requirement r)
                        {
                            return groupingType switch
                            {
                                GroupingType.IFC => r.SetName,
                                GroupingType.EN => r.SetNameEN,
                                GroupingType.CS => r.SetNameCS,
                                _ => r.SetName
                            };
                        }

                        string getName(Contracts.Requirement r)
                        {
                            return groupingType switch
                            {
                                GroupingType.IFC => r.Name,
                                GroupingType.EN => r.NameEN,
                                GroupingType.CS => r.NameCS,
                                _ => r.Name
                            };
                        }

                        var requirementSets = requirements.GroupBy(getSetName)
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
        public IActionResult GetGrouppedRequirements([FromQuery]OrderingType ordering = OrderingType.CS)
        {
            string getName(Contracts.Requirement r)
            {
                return ordering switch
                {
                    OrderingType.IFC => r.Name,
                    OrderingType.EN => r.NameEN,
                    OrderingType.CS => r.NameCS,
                    _ => r.Name
                };
            }

            string getSetName(Contracts.Requirement r)
            {
                return ordering switch
                {
                    OrderingType.IFC => r.SetName,
                    OrderingType.EN => r.SetNameEN,
                    OrderingType.CS => r.SetNameCS,
                    _ => r.SetName
                };
            }

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
                        .Distinct(PropertyEquality.Comparer)
                        .Select(r => new Contracts.Requirement(r, r.PartOfPsetTemplate.FirstOrDefault())))
                        .ToList();
                    if (requirementSets.Any())
                        result.Add(new Contracts.GrouppedRequirements(
                            item, 
                            requirementSets.OrderBy(getSetName).ThenBy(getName)
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

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum OrderingType
    {
        IFC,
        EN,
        CS
    }
}
