using System.Linq;

using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

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
    internal class ChangeResponseByHttpStatusCodeDocumentFilter<T> :
        IDocumentFilter
        where T : class
    {
        #region Fileds

        private readonly int _httpStatusCode;

        private readonly string _responseDescription;

        private readonly ResponseExampleOptions _responseExampleOption;

        private readonly T _responseExample;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="httpStatusCode">HTTP status code.</param>
        /// <param name="responseDescription">Response description.</param>
        /// <param name="responseExampleOption"><see cref="ResponseExampleOptions"/>.</param>
        /// <param name="responseExample">New example for response.</param>
        public ChangeResponseByHttpStatusCodeDocumentFilter(
            int httpStatusCode,
            string responseDescription,
            ResponseExampleOptions responseExampleOption,
            T responseExample)
        {
            _httpStatusCode = httpStatusCode;
            _responseDescription = responseDescription;
            _responseExampleOption = responseExampleOption;
            _responseExample = responseExample;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Apply filter.
        /// </summary>
        /// <param name="swaggerDoc"><see cref="OpenApiDocument"/>.</param>
        /// <param name="context"><see cref="DocumentFilterContext"/>.</param>
        public void Apply(
            OpenApiDocument swaggerDoc,
            DocumentFilterContext context)
        {
            if (context.SchemaRepository.Schemas.ContainsKey(typeof(T).Name))
            {
                var schema = context.SchemaRepository.Schemas[typeof(T).Name];
                foreach (var response in swaggerDoc.Paths.SelectMany(p => p.Value.Operations.SelectMany(o => o.Value.Responses)))
                {
                    if (response.Key == _httpStatusCode.ToString())
                    {
                        if (!string.IsNullOrWhiteSpace(_responseDescription))
                        {
                            response.Value.Description = _responseDescription;
                        }

                        if (response.Value.Content.ContainsKey("application/json"))
                        {
                            var jsonContent = response.Value.Content["application/json"];
                            switch (_responseExampleOption)
                            {
                                case ResponseExampleOptions.Clear:
                                    response.Value.Content.Remove("application/json");
                                    break;
                                case ResponseExampleOptions.AddNew:
                                    if (_responseExample != null)
                                    {
                                        var jsonExample = new OpenApiString(System.Text.Json.JsonSerializer.Serialize(_responseExample,
                                            new System.Text.Json.JsonSerializerOptions
                                            {
                                                WriteIndented = true,
                                                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                                            }));

                                        jsonContent.Example = jsonExample;
                                    }

                                    jsonContent.Schema = new OpenApiSchema
                                    {
                                        Reference = new OpenApiReference
                                        {
                                            Id = typeof(T).Name,
                                            Type = ReferenceType.Schema
                                        }
                                    };
                                    break;
                                case ResponseExampleOptions.None:
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            switch (_responseExampleOption)
                            {
                                case ResponseExampleOptions.AddNew:
                                    if (_responseExample != null)
                                    {
                                        var jsonExample = new OpenApiString(System.Text.Json.JsonSerializer.Serialize(_responseExample,
                                            new System.Text.Json.JsonSerializerOptions
                                            {
                                                WriteIndented = true,
                                                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                                            }));
                                        response.Value.Content.Add("application/json", new OpenApiMediaType
                                        {
                                            Example = jsonExample,
                                            Schema = new OpenApiSchema
                                            {
                                                Reference = new OpenApiReference
                                                {
                                                    Id = typeof(T).Name,
                                                    Type = ReferenceType.Schema
                                                }
                                            }
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