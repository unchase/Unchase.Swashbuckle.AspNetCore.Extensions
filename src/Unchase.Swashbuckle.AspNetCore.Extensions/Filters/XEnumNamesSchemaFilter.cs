using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Unchase.Swashbuckle.AspNetCore.Extensions.Extensions;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Filters
{
    public class XEnumNamesSchemaFilter : ISchemaFilter
    {
        #region Fields

        private readonly bool _includeXEnumDescriptions;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="includeXEnumDescriptions">If true - add "x-enumDescriptions" extensions from <see cref="DescriptionAttribute"/>.</param>
        public XEnumNamesSchemaFilter(bool includeXEnumDescriptions = false)
        {
            _includeXEnumDescriptions = includeXEnumDescriptions;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Apply the filter.
        /// </summary>
        /// <param name="schema"><see cref="OpenApiSchema"/>.</param>
        /// <param name="context"><see cref="SchemaFilterContext"/>.</param>
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            var typeInfo = context.Type.GetTypeInfo();
            var enumsArray = new OpenApiArray();
            var enumsDescriptionsArray = new OpenApiArray();
            if (typeInfo.IsEnum)
            {
                var names = Enum.GetNames(context.Type).Select(name => new OpenApiString(name));
                enumsArray.AddRange(names);
                if (!schema.Extensions.ContainsKey("x-enumNames") && enumsArray.Any())
                {
                    schema.Extensions.Add("x-enumNames", enumsArray);
                }

                if (this._includeXEnumDescriptions)
                {
                    enumsDescriptionsArray.AddRange(EnumTypeExtensions.GetEnumValuesDescription(context.Type));
                    if (!schema.Extensions.ContainsKey("x-enumDescriptions") && enumsDescriptionsArray.Any())
                    {
                        schema.Extensions.Add("x-enumDescriptions", enumsDescriptionsArray);
                    }
                }
                return;
            }
            
            // add "x-enumNames" for schema with generic types
            if (typeInfo.IsGenericType && !schema.Extensions.ContainsKey("x-enumNames"))
            {
                foreach (var genericArgumentType in typeInfo.GetGenericArguments())
                {
                    if (genericArgumentType.IsEnum)
                    {
                        if (schema.Properties?.Count > 0)
                        {
                            foreach (var schemaProperty in schema.Properties)
                            {
                                var schemaPropertyValue = schemaProperty.Value;
                                var propertySchema = context.SchemaRepository.Schemas.FirstOrDefault(s => schemaPropertyValue.AllOf.FirstOrDefault(a => a.Reference.Id == s.Key) != null).Value;
                                if (propertySchema != null)
                                {
                                    var names = Enum.GetNames(genericArgumentType).Select(name => new OpenApiString(name));
                                    enumsArray.AddRange(names);
                                    if (!schemaPropertyValue.Extensions.ContainsKey("x-enumNames") && enumsArray.Any())
                                    {
                                        schemaPropertyValue.Extensions.Add("x-enumNames", enumsArray);
                                    }

                                    if (this._includeXEnumDescriptions)
                                    {
                                        enumsDescriptionsArray.AddRange(EnumTypeExtensions.GetEnumValuesDescription(genericArgumentType));
                                        if (!schemaPropertyValue.Extensions.ContainsKey("x-enumDescriptions") && enumsDescriptionsArray.Any())
                                        {
                                            schemaPropertyValue.Extensions.Add("x-enumDescriptions", enumsDescriptionsArray);
                                        }
                                    }

                                    schemaPropertyValue.Description += propertySchema.AddEnumValuesDescription(this._includeXEnumDescriptions);
                                }
                            }
                        }
                    }
                }
            }
            
            if (schema.Properties?.Count > 0)
            {
                foreach (var schemaProperty in schema.Properties)
                {
                    var schemaPropertyValue = schemaProperty.Value;
                    var propertySchema = context.SchemaRepository.Schemas.FirstOrDefault(s => schemaPropertyValue.AllOf.FirstOrDefault(a => a.Reference.Id == s.Key) != null).Value;
                    if (propertySchema != null)
                    {
                        schemaPropertyValue.Description += propertySchema.AddEnumValuesDescription(this._includeXEnumDescriptions);
                    }
                }
            }
        }

        #endregion
    }
}
