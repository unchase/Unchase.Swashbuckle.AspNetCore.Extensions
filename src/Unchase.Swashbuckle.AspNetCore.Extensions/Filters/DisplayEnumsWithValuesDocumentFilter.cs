using System.Linq;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Unchase.Swashbuckle.AspNetCore.Extensions.Extensions;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Filters
{
    public class DisplayEnumsWithValuesDocumentFilter : IDocumentFilter
    {
        #region Fields

        private readonly bool _includeDescriptionFromAttribute;

        #endregion

        #region Constructors

        public DisplayEnumsWithValuesDocumentFilter(bool includeDescriptionFromAttribute = false)
        {
            _includeDescriptionFromAttribute = includeDescriptionFromAttribute;
        }

        #endregion

        #region Methods

        public void Apply(OpenApiDocument openApiDoc, DocumentFilterContext context)
        {
            foreach (var schemaDictionaryItem in openApiDoc.Components.Schemas)
            {
                var schema = schemaDictionaryItem.Value;
                schema.Description += schema.AddEnumValuesDescription(this._includeDescriptionFromAttribute);
            }

            if (openApiDoc.Paths.Count <= 0)
                return;

            // add enum descriptions to input parameters of every operation
            foreach (var parameter in openApiDoc.Paths.Values.SelectMany(v => v.Operations).SelectMany(op => op.Value.Parameters))
            {
                if (parameter.Schema.Reference == null)
                    continue;

                var componentReference = parameter.Schema.Reference.Id;
                var schema = openApiDoc.Components.Schemas[componentReference];

                parameter.Description += schema.AddEnumValuesDescription(this._includeDescriptionFromAttribute);
            }

            // add enum descriptions to request body
            foreach (var operation in openApiDoc.Paths.Values.SelectMany(v => v.Operations))
            {
                var requestBodyContents = operation.Value.RequestBody?.Content;
                if (requestBodyContents != null)
                {
                    foreach (var requestBodyContent in requestBodyContents)
                    {
                        if (requestBodyContent.Value.Schema?.Reference?.Id != null)
                        {
                            var schema = context.SchemaRepository.Schemas[requestBodyContent.Value.Schema?.Reference?.Id];
                            if (schema != null)
                            {
                                requestBodyContent.Value.Schema.Description = schema.Description;
                                requestBodyContent.Value.Schema.Extensions = schema.Extensions;
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}
