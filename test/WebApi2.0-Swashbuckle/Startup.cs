using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.Swagger;
using Unchase.Swashbuckle.AspNetCore.Extensions.Extensions;
using Unchase.Swashbuckle.AspNetCore.Extensions.Filters;

namespace WebApi2._0_Swashbuckle
{
#pragma warning disable CS1591
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
            //services.AddAuthentication();

            services.AddMvc()
                .AddJsonOptions(options => options.SerializerSettings.Formatting = Formatting.Indented)
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info {Title = "My API", Version = "v1"});

                options.AddEnumsWithValuesFixFilters(true);

                // or custom use:
                //options.SchemaFilter<XEnumNamesSchemaFilter>(); // add schema filter to fix enums (add 'x-enumNames') in schema
                //options.ParameterFilter<XEnumNamesParameterFilter>(); // add parameter filter to fix enums (add 'x-enumNames') in schema parameters
                //options.DocumentFilter<DisplayEnumsWithValuesDocumentFilter>(true); // add document filter to fix enums displaying in swagger document

                var filePath = Path.Combine(AppContext.BaseDirectory, "WebApi2.0-Swashbuckle.xml");
                options.IncludeXmlComments(filePath);

                // add Security information to each operation for OAuth2
                options.OperationFilter<SecurityRequirementsOperationFilter>();

                // enable swagger Annotations
                options.EnableAnnotations();
                // add action count into the SwaggerTag's descriptions
                options.DocumentFilter<AppendActionCountToTagSummaryDocumentFilter>();

                // if you're using the SecurityRequirementsOperationFilter, you also need to tell Swashbuckle you're using OAuth2
                options.AddSecurityDefinition("oauth2", new ApiKeyScheme
                {
                    Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
                    In = "header",
                    Name = "Authorization",
                    Type = "apiKey"
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

            app.UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((swaggerDoc, httpRequest) =>
                {
                    // hide all SwaggerDocument PathItems with added Security information for OAuth2 without accepted roles (for example, "AcceptedRole")
                    swaggerDoc.HidePathItemsWithoutAcceptedRoles(new List<string> {"AcceptedRole"});
                });
            });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
        }
    }
#pragma warning restore CS1591
}
