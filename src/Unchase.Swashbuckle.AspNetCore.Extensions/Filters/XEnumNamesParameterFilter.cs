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
    internal class XEnumNamesParameterFilter :
        IParameterFilter
    {
        #region Fields

        private readonly bool _includeXEnumDescriptions;
        private readonly bool _includeXEnumRemarks;
        private readonly string _xEnumNamesAlias;
        private readonly string _xEnumDescriptionsAlias;
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
        public XEnumNamesParameterFilter(
            IOptions<FixEnumsOptions> options,
            Action<FixEnumsOptions> configureOptions = null)
        {
            if (options.Value != null)
            {
                configureOptions?.Invoke(options.Value);
                _includeXEnumDescriptions = options.Value?.IncludeDescriptions ?? false;
                _includeXEnumRemarks = options.Value?.IncludeXEnumRemarks ?? false;
                _descriptionSources = options.Value?.DescriptionSource ?? DescriptionSources.DescriptionAttributes;
                _applyFiler = options.Value?.ApplyParameterFilter ?? false;
                _xEnumNamesAlias = options.Value?.XEnumNamesAlias;
                _xEnumDescriptionsAlias = options.Value?.XEnumDescriptionsAlias;
                foreach (var filePath in options.Value?.IncludedXmlCommentsPaths ?? new HashSet<string>())
                {
                    if (File.Exists(filePath))
                    {
                        _xmlNavigators.Add(new XPathDocument(filePath).CreateNavigator());
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
        public void Apply(
            OpenApiParameter parameter,
            ParameterFilterContext context)
        {
            if (!_applyFiler)
            {
                return;
            }

            var typeInfo = context.ParameterInfo?.ParameterType ?? context.PropertyInfo?.PropertyType;
            if (typeInfo == null)
            {
                return;
            }

            var enumsArray = new OpenApiArray();
            var enumsDescriptionsArray = new OpenApiArray();
            if (typeInfo.IsEnum)
            {
                var names = Enum
                    .GetNames(typeInfo)
                    .Select(name => (Enum.Parse(typeInfo, name), new OpenApiString(name)))
                    .GroupBy(x => x.Item1)
                    .Select(x => x.LastOrDefault().Item2)
                    .ToList();
                enumsArray.AddRange(names);
                if (!parameter.Extensions.ContainsKey(_xEnumNamesAlias) && enumsArray.Any())
                {
                    parameter.Extensions.Add(_xEnumNamesAlias, enumsArray);
                }

                if (_includeXEnumDescriptions)
                {
                    enumsDescriptionsArray.AddRange(EnumTypeExtensions
                        .GetEnumValuesDescription(typeInfo, _descriptionSources, _xmlNavigators, _includeXEnumRemarks)
                        .GroupBy(x => x.EnumValue)
                        .Select(x => x.LastOrDefault().EnumDescription));
                    if (!parameter.Extensions.ContainsKey(_xEnumDescriptionsAlias) && enumsDescriptionsArray.Any())
                    {
                        parameter.Extensions.Add(_xEnumDescriptionsAlias, enumsDescriptionsArray);
                    }
                }
            }
            else if (typeInfo.IsGenericType && !parameter.Extensions.ContainsKey(_xEnumNamesAlias))
            {
                foreach (var genericArgumentType in typeInfo.GetGenericArguments())
                {
                    if (genericArgumentType.IsEnum)
                    {
                        var names = Enum
                            .GetNames(genericArgumentType)
                            .Select(name => (Enum.Parse(genericArgumentType, name), new OpenApiString(name)))
                            .GroupBy(x => x.Item1)
                            .Select(x => x.LastOrDefault().Item2)
                            .ToList();
                        enumsArray.AddRange(names);
                        if (!parameter.Extensions.ContainsKey(_xEnumNamesAlias) && enumsArray.Any())
                        {
                            parameter.Extensions.Add(_xEnumNamesAlias, enumsArray);
                        }

                        if (_includeXEnumDescriptions)
                        {
                            enumsDescriptionsArray.AddRange(EnumTypeExtensions
                                .GetEnumValuesDescription(genericArgumentType, _descriptionSources, _xmlNavigators, _includeXEnumRemarks)
                                .GroupBy(x => x.EnumValue)
                                .Select(x => x.LastOrDefault().EnumDescription));
                            if (!parameter.Extensions.ContainsKey(_xEnumDescriptionsAlias) && enumsDescriptionsArray.Any())
                            {
                                parameter.Extensions.Add(_xEnumDescriptionsAlias, enumsDescriptionsArray);
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}
