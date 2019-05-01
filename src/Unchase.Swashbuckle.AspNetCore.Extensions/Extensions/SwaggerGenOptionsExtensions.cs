using System;
using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;
using Unchase.Swashbuckle.AspNetCore.Extensions.Filters;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Extensions
{
    public static class SwaggerGenOptionsExtensions
    {
        public static void EnumsWithValuesFixFilters(this SwaggerGenOptions swaggerGenOptions, bool includeDescriptionFromAttribute = false)
        {
            swaggerGenOptions.SchemaFilter<XEnumNamesSchemaFilter>();
            swaggerGenOptions.ParameterFilter<XEnumNamesParameterFilter>();
            swaggerGenOptions.DocumentFilter<DisplayEnumsWithValuesDocumentFilter>(includeDescriptionFromAttribute);
        }

        internal static string GetFieldAttributeDescription(this Type enumType, object enumField, int attributeNumber)
        {
            if (!enumType.IsEnum)
                return string.Empty;
            var memInfo = enumType.GetMember(enumField.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length > 0)
                return (attributes[attributeNumber] as DescriptionAttribute)?.Description ?? string.Empty;
            return string.Empty;
        }
    }
}
