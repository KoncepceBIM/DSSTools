using Microsoft.AspNet.OData;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LOIN.Server.Swagger
{
    internal class ODataParametersFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var method = context.MethodInfo;
            var enabled = method.GetCustomAttributes(typeof(EnableQueryAttribute), true).Length > 0;
            if (!enabled)
                return;

            operation.Parameters.Add(new OpenApiParameter {
                Name = "$select",
                Description = "OData select expression",
                In = ParameterLocation.Query,
                Required = false,
                Schema = new OpenApiSchema { Type = "string" }
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "$filter",
                Description = "OData filter expression",
                In = ParameterLocation.Query,
                Required = false,
                Schema = new OpenApiSchema { Type = "string" }
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "$orderby",
                Description = "OData order-by expression",
                In = ParameterLocation.Query,
                Required = false,
                Schema = new OpenApiSchema { Type = "string" }
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "$skip",
                Description = "OData 'skip' attribute. Usable for paging.",
                In = ParameterLocation.Query,
                Required = false,
                Schema = new OpenApiSchema { Type = "integer" }
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "$top",
                Description = "OData 'top' attribute. Usable for paging.",
                In = ParameterLocation.Query,
                Required = false,
                Schema = new OpenApiSchema { Type = "integer" }
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "$apply",
                Description = "OData 'apply' attribute. Usable for grouping, aggregations etc.",
                In = ParameterLocation.Query,
                Required = false,
                Schema = new OpenApiSchema { Type = "string" }
            });
        }
    }
}
