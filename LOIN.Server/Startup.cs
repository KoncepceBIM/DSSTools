using LOIN.Server.Services.Implementations;
using LOIN.Server.Services.Interfaces;
using LOIN.Server.Swagger;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace LOIN.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // repositories are kept in the cache
            services.AddMemoryCache();

            services.AddSingleton<ILoinRepository, LoinRepository>();

            services.AddRouting(options => options.LowercaseUrls = true);

            services.AddOData();
            services.AddControllers(options =>
            {
                foreach (var outputFormatter in options.OutputFormatters.OfType<ODataOutputFormatter>().Where(_ => _.SupportedMediaTypes.Count == 0))
                {
                    outputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/prs.odatatestxx-odata"));
                }
                foreach (var inputFormatter in options.InputFormatters.OfType<ODataInputFormatter>().Where(_ => _.SupportedMediaTypes.Count == 0))
                {
                    inputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/prs.odatatestxx-odata"));
                }
            }).AddNewtonsoftJson();

            // TODO: Configure for a specific web app
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowAnyOrigin();
                    });
            });

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(opts => {
                opts.OperationFilter<ODataParametersFilter>();
                opts.OperationFilter<LoinContextParameterFilter>();
                opts.OperationFilter<FileResultContentTypeOperationFilter>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoinRepository repository)
        {
            // top level exception handler
            app.Use(async (ctx, next) => {
                try
                {
                    await next();
                }
                catch (Exception e)
                {
                    var log = ctx.RequestServices.GetRequiredService<ILogger<Startup>>();
                    log.LogError(e, "Failed request: {path}", ctx.Request.Path.Value);


                    ctx.Response.StatusCode = 400;
                    var err = new ProblemDetails 
                    {
                        Title = $"Failed request at '{ctx.Request.Path.Value}'. See the log for more details.",
                        Status = 400,
                        Detail = e.Message
                    };
                    var data = JsonSerializer.Serialize(err);
                    await ctx.Response.WriteAsync(data);
                }
            });

            // TODO: Re-enable for production
            // app.UseHttpsRedirection();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "LOIN API v1");
            });

            app.UseRouting();

            app.UseCors();

            app.UseAuthorization();

            app.Use(async (context, next) =>
            {
                var routeData = context.GetRouteData();
                var repositoryId = routeData?.Values["repositoryId"]?.ToString();
                if (!string.IsNullOrEmpty(repositoryId))
                {
                    // this will always return the latest version
                    if (string.Equals(repositoryId, "latest", StringComparison.OrdinalIgnoreCase))
                    {
                        repositoryId = repository.GetRepositoryIds().OrderBy(id => id).LastOrDefault();
                    }

                    var repo = await repository.OpenRepository(repositoryId);
                    context.Items[Constants.RepositoryContextKey] = repo;
                }

                await next();
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.EnableDependencyInjection();
                endpoints.Select().Filter().OrderBy().Count().MaxTop(1000);
            });
        }
    }
}
