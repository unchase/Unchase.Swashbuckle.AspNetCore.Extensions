using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using TodoApi.Models;
using Unchase.Swashbuckle.AspNetCore.Extensions.Extensions;
using Unchase.Swashbuckle.AspNetCore.Extensions.Filters;
using WebApi3._1_Swashbuckle.Controllers;
using WebApi3._1_Swashbuckle.Models;

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

                #region AddEnumsWithValuesFixFilters

                // if you want to add xml comments into the swagger documentation, first of all add:
                var filePath = Path.Combine(AppContext.BaseDirectory, "WebApi3.1-Swashbuckle.xml");
                options.IncludeXmlComments(filePath);

                // Add filters to fix enums
                options.AddEnumsWithValuesFixFilters(true);

                // or custom use:
                //options.SchemaFilter<XEnumNamesSchemaFilter>(true); // add schema filter to fix enums (add 'x-enumNames' for NSwag) in schema
                //options.ParameterFilter<XEnumNamesParameterFilter>(true); // add parameter filter to fix enums (add 'x-enumNames' for NSwag) in schema parameters
                //options.DocumentFilter<DisplayEnumsWithValuesDocumentFilter>(true); // add document filter to fix enums displaying in swagger document

                #endregion

                #region HidePathsAndDefinitionsByRolesDocumentFilter

                // remove Paths and Components from OpenApi documentation without accepted roles
                options.DocumentFilter<HidePathsAndDefinitionsByRolesDocumentFilter>(new List<string> { "AcceptedRole" });

                #endregion

                #region AppendActionCountToTagSummaryDocumentFilter

                // enable openApi Annotations
                options.EnableAnnotations();

                // add action count into the SwaggerTag's descriptions
                // you can use it after "HidePathsAndDefinitionsByRolesDocumentFilter"
                options.DocumentFilter<AppendActionCountToTagSummaryDocumentFilter>();

                #endregion

                #region TagOrderByNameDocumentFilter

                // order tags by name
                options.DocumentFilter<TagOrderByNameDocumentFilter>();

                #endregion

                #region ChangeAllResponsesByHttpStatusCode

                // change responses for specific HTTP status code ("200")
                options.ChangeAllResponsesByHttpStatusCode(
                    httpStatusCode: 200,
                    responseDescription: "200 status code description",
                    responseExampleOption: ResponseExampleOptions.AddNew, // add new response examples
                    responseExample: new TodoItem { Tag = Tag.Workout, Id = 111, IsComplete = false, Name = "test" }); // some class for response examples

                // change responses for specific HTTP status code ("400" (HttpStatusCode.BadRequest))
                options.ChangeAllResponsesByHttpStatusCode(
                    httpStatusCode: HttpStatusCode.BadRequest,
                    responseDescription: "400 status code description",
                    responseExampleOption: ResponseExampleOptions.Clear, // claer response examples
                    responseExample: new ComplicatedClass()); // some class for response examples

                // change responses for specific HTTP status code ("201" (StatusCodes.Status201Created))
                options.ChangeAllResponsesByHttpStatusCode(
                    httpStatusCode: StatusCodes.Status201Created,
                    responseDescription: "201 status code description",
                    responseExampleOption: ResponseExampleOptions.None, // do nothing with response examples
                    responseExample: new ComplicatedClass()); // some class for response examples

                #endregion
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((openApiDoc, httpRequest) =>
                {
                    // remove Paths and Components from OpenApi documentation for specific controller action without accepted roles
                    openApiDoc.RemovePathsAndComponentsWithoutAcceptedRolesFor<HidedController>(controller => nameof(controller.HidedAction), new List<string> {"AcceptedRole"});
                });
            });

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
