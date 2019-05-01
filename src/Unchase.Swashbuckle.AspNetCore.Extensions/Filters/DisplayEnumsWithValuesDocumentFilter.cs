using System;
using System.Collections.Generic;
using System.Linq;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Unchase.Swashbuckle.AspNetCore.Extensions.Extensions;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Filters
{
    public class DisplayEnumsWithValuesDocumentFilter : IDocumentFilter
    {
        private readonly bool _includeDescriptionFromAttribute;

        public DisplayEnumsWithValuesDocumentFilter(bool includeDescriptionFromAttribute = false)
        {
            _includeDescriptionFromAttribute = includeDescriptionFromAttribute;
        }

        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (var schemaDictionaryItem in swaggerDoc.Definitions)
            {
                var schema = schemaDictionaryItem.Value;
                foreach (var propertyDictionaryItem in schema.Properties)
                {
                    var property = propertyDictionaryItem.Value;
                    var propertyEnums = property.Enum;
                    if (propertyEnums != null && propertyEnums.Count > 0)
                        property.Description += DescribeEnum(propertyEnums);
                }
            }

            if (swaggerDoc.Paths.Count <= 0)
                return;

            // add enum descriptions to input parameters
            foreach (var pathItem in swaggerDoc.Paths.Values)
            {
                DescribeEnumParameters(pathItem.Parameters, _includeDescriptionFromAttribute);

                // head, patch, options, delete left out
                var possibleParameterizedOperations = new List<Operation> { pathItem.Get, pathItem.Post, pathItem.Put, pathItem.Delete, pathItem.Patch };
                possibleParameterizedOperations.FindAll(x => x != null)
                    .ForEach(x => DescribeEnumParameters(x.Parameters, _includeDescriptionFromAttribute));
            }
        }

        private static void DescribeEnumParameters(IList<IParameter> parameters, bool includeDescriptionFromAttribute = false)
        {
            if (parameters == null)
                return;
            foreach (var param in parameters)
            {
                if (param.Extensions.ContainsKey("enum") && param.Extensions["enum"] is IList<object> paramEnums && paramEnums.Count > 0)
                    param.Description += DescribeEnum(paramEnums, includeDescriptionFromAttribute);
                else if (param is NonBodyParameter nbParam && nbParam.Enum?.Any() == true)
                    param.Description += DescribeEnum(nbParam.Enum, includeDescriptionFromAttribute);
            }
        }

        private static string DescribeEnum(IEnumerable<object> enums, bool includeDescriptionFromAttribute = false)
        {
            var enumDescriptions = new List<string>();
            Type type = null;
            foreach (var enumOption in enums)
            {
                if (type == null)
                    type = enumOption.GetType();
                if (includeDescriptionFromAttribute)
                {
                    var enumOptionDescription = type.GetFieldAttributeDescription(enumOption, 0);
                    enumDescriptions.Add(string.IsNullOrWhiteSpace(enumOptionDescription)
                        ? $"{Convert.ChangeType(enumOption, type.GetEnumUnderlyingType())} = {Enum.GetName(type, enumOption)}"
                        : $"{Convert.ChangeType(enumOption, type.GetEnumUnderlyingType())} = {Enum.GetName(type, enumOption)} ({enumOptionDescription})");
                }
                else
                    enumDescriptions.Add($"{Convert.ChangeType(enumOption, type.GetEnumUnderlyingType())} = {Enum.GetName(type, enumOption)}");
            }
            return $"{Environment.NewLine}{string.Join(Environment.NewLine, enumDescriptions)}";
        }
    }
}
