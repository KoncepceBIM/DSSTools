using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOIN.Server.Swagger
{
    internal class LoinContextParameterFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var method = context.MethodInfo;
            var enabled = method.GetCustomAttributes(typeof(EnableLoinContextAttribute), true).Length > 0;
            if (!enabled)
                return;

            var isPost = string.Equals(context.ApiDescription.HttpMethod, "post", StringComparison.OrdinalIgnoreCase);
            if (isPost)
            {
                var body = operation.RequestBody ?? new OpenApiRequestBody();
                if (body.Content == null)
                    body.Content = new Dictionary<string, OpenApiMediaType>();
                if (!body.Content.TryGetValue("multipart/form-data", out var formData))
                {
                    formData = new OpenApiMediaType();
                    body.Content.Add("multipart/form-data", formData);
                }

                if (formData.Schema == null)
                    formData.Schema = new OpenApiSchema() { Type = "object"};
                var schema = formData.Schema;

                if (schema.Properties == null)
                    schema.Properties = new Dictionary<string, OpenApiSchema>();

                var properties = schema.Properties;

                properties.Add("actors", new OpenApiSchema { Type = "string", Description = "Coma separated list of actors (id) for the context filtering" });
                properties.Add("reasons", new OpenApiSchema { Type = "string", Description = "Coma separated list of reasons (id) for the context filtering" });
                properties.Add("breakdown", new OpenApiSchema { Type = "string", Description = "Coma separated list of breakdown items (id) for the context filtering" });
                properties.Add("milestones", new OpenApiSchema { Type = "string", Description = "Coma separated list of milestones (id) for the context filtering" });

                operation.RequestBody = body;
                return;
            }

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
