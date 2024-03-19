using Microsoft.OpenApi.Models;
using System.Net;
using Unchase.Swashbuckle.AspNetCore.Extensions.Extensions;
using Unchase.Swashbuckle.AspNetCore.Extensions.Filters;
using Unchase.Swashbuckle.AspNetCore.Extensions.Options;
using Unchase.Swashbuckle.AspNetCore.Extensions.Tests.Controllers;
using Unchase.Swashbuckle.AspNetCore.Extensions.Tests.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register the Swagger generator
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // use it if you want to hide Paths and Definitions from OpenApi documentation correctly
    options.UseAllOfToExtendReferenceSchemas();

    // if you want to add xml comments from summary and remarks into the swagger documentation, first of all add:
    // you can exclude remarks for concrete types
    var xmlFilePath = Path.Combine(AppContext.BaseDirectory, "Unchase.Swashbuckle.AspNetCore.Extensions.Tests.xml");
    options.IncludeXmlCommentsWithRemarks(filePath: xmlFilePath, includeControllerXmlComments: false,
        typeof(ComplicatedClass),
        typeof(InnerEnum));

    var xmlFilePath1 = xmlFilePath;
    xmlFilePath = Path.Combine(AppContext.BaseDirectory, "WebApi8-Swashbuckle.xml");
    options.IncludeXmlCommentsWithRemarks(filePath: xmlFilePath, includeControllerXmlComments: false,
        typeof(ComplicatedClass),
        typeof(InnerEnum));

    // if you want to add xml comments from inheritdocs (from summary and remarks) into the swagger documentation, add:
    // you can exclude remarks for concrete types
    options.IncludeXmlCommentsFromInheritDocs(includeRemarks: true);

    // options.IncludeXmlCommentsWithRemarks(filePath: xmlFilePath, includeControllerXmlComments: false, () => new[] { typeof(InnerEnum) });

    // or add without remarks
    //options.IncludeXmlComments(xmlFilePath);

    #region AddEnumsWithValuesFixFilters

    // Add filters to fix enums
    // use by default:
    options.AddEnumsWithValuesFixFilters();

    // or configured:
    options.AddEnumsWithValuesFixFilters(o =>
    {
        // add schema filter to fix enums (add 'x-enumNames' for NSwag or its alias from XEnumNamesAlias) in schema
        o.ApplySchemaFilter = true;

        // alias for replacing 'x-enumNames' in swagger document
        o.XEnumNamesAlias = "x-enum-varnames";

        // alias for replacing 'x-enumDescriptions' in swagger document
        o.XEnumDescriptionsAlias = "x-enum-descriptions";

        // add parameter filter to fix enums (add 'x-enumNames' for NSwag or its alias from XEnumNamesAlias) in schema parameters
        o.ApplyParameterFilter = true;

        // add document filter to fix enums displaying in swagger document
        o.ApplyDocumentFilter = true;

        // add descriptions from DescriptionAttribute or xml-comments to fix enums (add 'x-enumDescriptions' for schema extensions) for applied filters
        o.IncludeDescriptions = true;

        // add remarks for descriptions from xml-comments
        o.IncludeXEnumRemarks = true;

        // get descriptions from DescriptionAttribute then from xml-comments
        o.DescriptionSource = DescriptionSources.DescriptionAttributesThenXmlComments;

        // new line for enum values descriptions
        // o.NewLine = Environment.NewLine;
        o.NewLine = "\n";

        // get descriptions from xml-file comments on the specified path
        // should use "options.IncludeXmlComments(xmlFilePath);" before
        o.IncludeXmlCommentsFrom(xmlFilePath);
        o.IncludeXmlCommentsFrom(xmlFilePath1);
        // the same for another xml-files...
    });

    #endregion

    #region HidePathsAndDefinitionsByRolesDocumentFilter

    // remove Paths and Components from OpenApi documentation without accepted roles
    options.DocumentFilter<HidePathsAndDefinitionsByRolesDocumentFilter>(new List<string> { "AcceptedRole" });

    #endregion

    #region AppendActionCountToTagSummaryDocumentFilter

    // enable openApi Annotations
    options.EnableAnnotations();

    // add action count (with message template) into the SwaggerTag's descriptions
    // you can use it after "HidePathsAndDefinitionsByRolesDocumentFilter"
    options.DocumentFilter<AppendActionCountToTagSummaryDocumentFilter>("(the count of actions: {0})");

    #endregion

    #region TagOrderByNameDocumentFilter

    // order tags by name
    options.DocumentFilter<TagOrderByNameDocumentFilter>();

    #endregion

    #region ChangeAllResponsesByHttpStatusCode

    // change responses for specific HTTP status code ("200")
    //options.ChangeAllResponsesByHttpStatusCode(
    //    httpStatusCode: 200,
    //    responseDescription: "200 status code description",
    //    responseExampleOption: ResponseExampleOptions.AddNew, // add new response examples
    //    responseExample: new TodoItem { Tag = Tag.Workout, Id = 111, IsComplete = false, Name = "test" }); // some class for response examples

    // change responses for specific HTTP status code ("400" (HttpStatusCode.BadRequest))
    options.ChangeAllResponsesByHttpStatusCode(httpStatusCode: HttpStatusCode.BadRequest,
        responseDescription: "400 status code description",
        responseExampleOption: ResponseExampleOptions.Clear, // clear response examples
        responseExample: new ComplicatedClass()); // some class for response examples

    // change responses for specific HTTP status code ("201" (StatusCodes.Status201Created))
    options.ChangeAllResponsesByHttpStatusCode(httpStatusCode: StatusCodes.Status201Created,
        responseDescription: "201 status code description",
        responseExampleOption: ResponseExampleOptions.None, // do nothing with response examples
        responseExample: new ComplicatedClass()); // some class for response examples

    #endregion
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// enable middleware to serve generated Swagger as a JSON endpoint.
app.UseSwagger(c =>
{
    c.PreSerializeFilters.Add((openApiDoc, _) =>
    {
        // remove Paths and Components from OpenApi documentation for specific controller action without accepted roles
        openApiDoc.RemovePathsAndComponentsWithoutAcceptedRolesFor<HidedController>(controller => nameof(controller.HidedAction), new List<string> { "AcceptedRole" });

        // or
        //openApiDoc.RemovePathsAndComponentsWithoutAcceptedRolesFor<HidedController>(nameof(HidedController.HidedAction), new List<string> { "AcceptedRole" });


        // remove Paths and Components from OpenApi documentation for all controller actions without accepted roles
        openApiDoc.RemovePathsAndComponentsWithoutAcceptedRolesForController<TodoController>(new List<string> { "AcceptedRole" });

        // or you can get accepted roles by httpRequest like this:
        //openApiDoc.RemovePathsAndComponentsWithoutAcceptedRolesForController<TodoController>(GetAcceptedRolesByRemoteIp(httpRequest.HttpContext.Connection.RemoteIpAddress));
    });
});

// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
// specifying the Swagger JSON endpoint.
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
});

app.UseRouting();

#pragma warning disable ASP0014
app.UseEndpoints(endpoints => endpoints.MapControllers());
#pragma warning restore ASP0014

app.Run();