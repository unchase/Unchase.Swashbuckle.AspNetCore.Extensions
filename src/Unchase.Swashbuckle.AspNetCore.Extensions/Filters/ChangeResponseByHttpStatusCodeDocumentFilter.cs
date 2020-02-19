using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Filters
{
    /// <summary>
    /// Options for response example.
    /// </summary>
    public enum ResponseExampleOptions
    {
        /// <summary>
        /// Clear example.
        /// </summary>
        Clear = 0,

        /// <summary>
        /// Add (replace) example.
        /// </summary>
        AddNew = 1,

        /// <summary>
        /// Do nothing.
        /// </summary>
        None = 2
    }

    /// <summary>
    /// Document filter for changing responses by specific http status codes in OpenApi document.
    /// </summary>
    /// <typeparam name="T">Type of response example.</typeparam>
    internal class ChangeResponseByHttpStatusCodeDocumentFilter<T> : IDocumentFilter where T : class
    {
        #region Fileds

        private int _httpStatusCode;

        private string _responseDescription;

        private ResponseExampleOptions _responseExampleOption;

        private T _responseExample;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="httpStatusCode">HTTP status code.</param>
        /// <param name="responseDescription">Response description.</param>
        /// <param name="responseExampleOption"><see cref="ResponseExampleOptions"/>.</param>
        /// <param name="responseExample">New example for response.</param>
        public ChangeResponseByHttpStatusCodeDocumentFilter(int httpStatusCode, string responseDescription, ResponseExampleOptions responseExampleOption, T responseExample)
        {
            this._httpStatusCode = httpStatusCode;
            this._responseDescription = responseDescription;
            this._responseExampleOption = responseExampleOption;
            this._responseExample = responseExample;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Apply filter.
        /// </summary>
        /// <param name="swaggerDoc"><see cref="OpenApiDocument"/>.</param>
        /// <param name="context"><see cref="DocumentFilterContext"/>.</param>
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            if (context.SchemaRepository.Schemas.ContainsKey(typeof(T).Name))
            {
                var schema = context.SchemaRepository.Schemas[typeof(T).Name];
                foreach (var response in swaggerDoc.Paths.SelectMany(p => p.Value.Operations.SelectMany(o => o.Value.Responses)))
                {
                    if (response.Key == this._httpStatusCode.ToString())
                    {
                        if (!string.IsNullOrWhiteSpace(this._responseDescription))
                            response.Value.Description = this._responseDescription;

                        if (response.Value.Content.ContainsKey("application/json"))
                        {
                            var jsonContent = response.Value.Content["application/json"];
                            switch (this._responseExampleOption)
                            {
                                case ResponseExampleOptions.Clear:
                                    response.Value.Content.Remove("application/json");
                                    break;
                                case ResponseExampleOptions.AddNew:
                                    if (this._responseExample != null)
                                        jsonContent.Example = new OpenApiString(System.Text.Json.JsonSerializer.Serialize(this._responseExample, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
                                    jsonContent.Schema = schema;
                                    break;
                                case ResponseExampleOptions.None:
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            switch (this._responseExampleOption)
                            {
                                case ResponseExampleOptions.AddNew:
                                    if (this._responseExample != null)
                                    {
                                        response.Value.Content.Add("application/json", new OpenApiMediaType()
                                        {
                                            Example = new OpenApiString(System.Text.Json.JsonSerializer.Serialize(this._responseExample, new System.Text.Json.JsonSerializerOptions { WriteIndented = true })),
                                            Schema = schema
                                        });
                                    }
                                    break;
                                case ResponseExampleOptions.Clear:
                                case ResponseExampleOptions.None:
                                default:
                                    break;
                            }
                            
                        }
                    }
                }
            }
        }

        #endregion
    }
}
