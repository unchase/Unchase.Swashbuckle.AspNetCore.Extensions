using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Unchase.Swashbuckle.AspNetCore.Extensions.Extensions;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Filters
{
    public class XEnumNamesParameterFilter : IParameterFilter
    {
        #region Fields

        private readonly bool _includeXEnumDescriptions;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="includeXEnumDescriptions">If true - add "x-enumDescriptions" extensions from <see cref="DescriptionAttribute"/>.</param>
        public XEnumNamesParameterFilter(bool includeXEnumDescriptions = false)
        {
            _includeXEnumDescriptions = includeXEnumDescriptions;
        }

        #endregion

        #region Methods

        public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
        {
            var typeInfo = context.ParameterInfo?.ParameterType ?? context.PropertyInfo.PropertyType;
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
                    enumsDescriptionsArray.AddRange(EnumTypeExtensions.GetEnumValuesDescription(typeInfo));
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
                            enumsDescriptionsArray.AddRange(EnumTypeExtensions.GetEnumValuesDescription(genericArgumentType));
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
