using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.XPath;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Unchase.Swashbuckle.AspNetCore.Extensions.Extensions;
using Unchase.Swashbuckle.AspNetCore.Extensions.Options;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Filters
{
    internal class XEnumNamesParameterFilter : IParameterFilter
    {
        #region Fields

        private readonly bool _includeXEnumDescriptions;
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
        public XEnumNamesParameterFilter(IOptions<FixEnumsOptions> options, Action<FixEnumsOptions> configureOptions = null)
        {
            if (options.Value != null)
            {
                configureOptions?.Invoke(options.Value);
                this._includeXEnumDescriptions = options.Value?.IncludeDescriptions ?? false;
                this._descriptionSources = options.Value?.DescriptionSource ?? DescriptionSources.DescriptionAttributes;
                this._applyFiler = options.Value?.ApplyParameterFilter ?? false;
                foreach (var filePath in options.Value?.IncludedXmlCommentsPaths)
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
        /// <param name="parameter"><see cref="OpenApiParameter"/>.</param>
        /// <param name="context"><see cref="ParameterFilterContext"/>.</param>
        public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
        {
            if (!this._applyFiler)
                return;

            var typeInfo = context.ParameterInfo?.ParameterType ?? context.PropertyInfo?.PropertyType;
            if (typeInfo == null)
                return;

            var enumsArray = new OpenApiArray();
            var enumsDescriptionsArray = new OpenApiArray();
            if (typeInfo.IsEnum)
            {
                var names = Enum.GetNames(typeInfo).Select(name => new OpenApiString(name));
                enumsArray.AddRange(names);
                if (!parameter.Extensions.ContainsKey("x-enumNames") && enumsArray.Any())
                {
                    parameter.Extensions.Add("x-enumNames", enumsArray);
                }

                if (this._includeXEnumDescriptions)
                {
                    enumsDescriptionsArray.AddRange(EnumTypeExtensions.GetEnumValuesDescription(typeInfo, this._descriptionSources, this._xmlNavigators));
                    if (!parameter.Extensions.ContainsKey("x-enumDescriptions") && enumsDescriptionsArray.Any())
                    {
                        parameter.Extensions.Add("x-enumDescriptions", enumsDescriptionsArray);
                    }
                }
            }
            else if (typeInfo.IsGenericType && !parameter.Extensions.ContainsKey("x-enumNames"))
            {
                foreach (var genericArgumentType in typeInfo.GetGenericArguments())
                {
                    if (genericArgumentType.IsEnum)
                    {
                        var names = Enum.GetNames(genericArgumentType).Select(name => new OpenApiString(name));
                        enumsArray.AddRange(names);
                        if (!parameter.Extensions.ContainsKey("x-enumNames") && enumsArray.Any())
                        {
                            parameter.Extensions.Add("x-enumNames", enumsArray);
                        }

                        if (this._includeXEnumDescriptions)
                        {
                            enumsDescriptionsArray.AddRange(EnumTypeExtensions.GetEnumValuesDescription(genericArgumentType, this._descriptionSources, this._xmlNavigators));
                            if (!parameter.Extensions.ContainsKey("x-enumDescriptions") && enumsDescriptionsArray.Any())
                            {
                                parameter.Extensions.Add("x-enumDescriptions", enumsDescriptionsArray);
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}
