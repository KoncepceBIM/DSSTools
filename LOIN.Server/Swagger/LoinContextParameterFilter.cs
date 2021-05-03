using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOIN.Server.Swagger
{
    public class LoinContextParameterFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var method = context.MethodInfo;
            var enabled = method.GetCustomAttributes(typeof(EnableLoinContextAttribute), true).Length > 0;
            if (!enabled)
                return;

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "actors",
                Description = "Coma separated list of actors (id) for the context filtering",
                In = ParameterLocation.Query,
                Required = false,
                Schema = new OpenApiSchema { Type = "string" }
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "reasons",
                Description = "Coma separated list of reasons (id) for the context filtering",
                In = ParameterLocation.Query,
                Required = false,
                Schema = new OpenApiSchema { Type = "string" }
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "breakdown",
                Description = "Coma separated list of breakdown items (id) for the context filtering",
                In = ParameterLocation.Query,
                Required = false,
                Schema = new OpenApiSchema { Type = "string" }
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "milestones",
                Description = "Coma separated list of milestones (id) for the context filtering",
                In = ParameterLocation.Query,
                Required = false,
                Schema = new OpenApiSchema { Type = "string" }
            });
        }
    }
}
