using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.XPath;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Unchase.Swashbuckle.AspNetCore.Extensions.Extensions;
using Unchase.Swashbuckle.AspNetCore.Extensions.Options;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Filters
{
    internal class XEnumNamesSchemaFilter : ISchemaFilter
    {
        #region Fields

        private readonly bool _includeXEnumDescriptions;
        private readonly bool _includeXEnumRemarks;
        private readonly DescriptionSources _descriptionSources;
        private readonly bool _applyFiler;
        private readonly HashSet<XPathNavigator> _xmlNavigators = new HashSet<XPathNavigator>();

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="options"><see cref="FixEnumsOptions"/>.</param>
        /// <param name="configureOptions">An <see cref="Action{FixEnumsOptions}"/> to configure options for filter.</param>
        public XEnumNamesSchemaFilter(IOptions<FixEnumsOptions> options, Action<FixEnumsOptions> configureOptions = null)
        {
            if (options.Value != null)
            {
                configureOptions?.Invoke(options.Value);
                this._includeXEnumDescriptions = options.Value.IncludeDescriptions;
                this._includeXEnumRemarks = options.Value.IncludeXEnumRemarks;
                this._descriptionSources = options.Value?.DescriptionSource ?? DescriptionSources.DescriptionAttributes;
                this._applyFiler = options.Value.ApplySchemaFilter;
                foreach (var filePath in options.Value.IncludedXmlCommentsPaths)
                {
                    if (File.Exists(filePath))
                    {
                        this._xmlNavigators.Add(new XPathDocument(filePath).CreateNavigator());
                    }
                }
            }
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
            if (!this._applyFiler)
                return;

            var typeInfo = context.Type.GetTypeInfo();
            var enumsArray = new OpenApiArray();
            var enumsDescriptionsArray = new OpenApiArray();
            if (typeInfo.IsEnum)
            {
                var names = Enum.GetNames(context.Type).Select(name => new OpenApiString(name)).ToList();
                enumsArray.AddRange(names);
                if (!schema.Extensions.ContainsKey("x-enumNames") && enumsArray.Any())
                {
                    schema.Extensions.Add("x-enumNames", enumsArray);
                }

                if (this._includeXEnumDescriptions)
                {
                    enumsDescriptionsArray.AddRange(EnumTypeExtensions.GetEnumValuesDescription(context.Type, this._descriptionSources, this._xmlNavigators, this._includeXEnumRemarks));
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
                                        enumsDescriptionsArray.AddRange(EnumTypeExtensions.GetEnumValuesDescription(genericArgumentType, this._descriptionSources, this._xmlNavigators, this._includeXEnumRemarks));
                                        if (!schemaPropertyValue.Extensions.ContainsKey("x-enumDescriptions") && enumsDescriptionsArray.Any())
                                        {
                                            schemaPropertyValue.Extensions.Add("x-enumDescriptions", enumsDescriptionsArray);
                                        }
                                    }

                                    var description = propertySchema.AddEnumValuesDescription(this._includeXEnumDescriptions);
                                    if (description != null)
                                    {
                                        if (schemaPropertyValue.Description == null)
                                        {
                                            schemaPropertyValue.Description = description;
                                        }
                                        else if (!schemaPropertyValue.Description.Contains(description))
                                        {
                                            schemaPropertyValue.Description += description;
                                        }
                                    }
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
                    var description = propertySchema?.AddEnumValuesDescription(this._includeXEnumDescriptions);
                    if (description != null)
                    {
                        if (schemaPropertyValue.Description == null)
                        {
                            schemaPropertyValue.Description = description;
                        }
                        else if (!schemaPropertyValue.Description.Contains(description))
                        {
                            schemaPropertyValue.Description += description;
                        }
                    }
                }
            }
        }

        #endregion
    }
}
