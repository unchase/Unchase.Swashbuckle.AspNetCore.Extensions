using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using Unchase.Swashbuckle.AspNetCore.Extensions.Extensions;
using Unchase.Swashbuckle.AspNetCore.Extensions.Filters;

namespace WebApi3._1_Swashbuckle
{
#pragma warning disable CS1591
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

                // if you want to add xml comments into the swagger documentation, first of all add:
                var filePath = Path.Combine(AppContext.BaseDirectory, "WebApi3.1-Swashbuckle.xml");
                options.IncludeXmlComments(filePath);

                // Add filters to fix enums
                options.AddEnumsWithValuesFixFilters(true);

                // or custom use:
                //options.SchemaFilter<XEnumNamesSchemaFilter>(true); // add schema filter to fix enums (add 'x-enumNames' for NSwag) in schema
                //options.ParameterFilter<XEnumNamesParameterFilter>(true); // add parameter filter to fix enums (add 'x-enumNames' for NSwag) in schema parameters
                //options.DocumentFilter<DisplayEnumsWithValuesDocumentFilter>(true); // add document filter to fix enums displaying in swagger document


                // remove Paths and Defenitions from OpenApi documentation without accepted roles
                options.DocumentFilter<HidePathsAndDefinitionsByRolesDocumentFilter>(new List<string> { "AcceptedRole" });


                // enable openApi Annotations
                options.EnableAnnotations();

                // add action count into the SwaggerTag's descriptions
                // you can use it after "HidePathsAndDefinitionsByRolesDocumentFilter"
                options.DocumentFilter<AppendActionCountToTagSummaryDocumentFilter>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
#pragma warning restore CS1591
}
