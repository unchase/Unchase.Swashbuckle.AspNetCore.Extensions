using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System;
using System.Text;
using System.Collections.Generic;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Extensions
{
    internal static class EnumTypeExtensions
    {
        internal static string GetDescriptionFromEnumOption(Type enumOptionType, object enumOption)
        {
            return enumOptionType.GetFieldAttributeDescription(enumOption, 0);
        }

        internal static List<OpenApiString> GetEnumValuesDescription(Type enumType)
        {
            var enumsDescriptions = new List<OpenApiString>();
            foreach (var enumValue in Enum.GetValues(enumType))
            {
                var enumDescription = GetDescriptionFromEnumOption(enumType, enumValue);
                enumsDescriptions.Add(new OpenApiString(enumDescription));
            }
            return enumsDescriptions;
        }

        internal static string AddEnumValuesDescription(this OpenApiSchema schema, bool includeDescriptionFromAttribute = false)
        {
            if (schema.Enum == null || schema.Enum.Count == 0)
                return null;

            if (!schema.Extensions.ContainsKey("x-enumNames") || ((OpenApiArray)schema.Extensions["x-enumNames"]).Count != schema.Enum.Count)
                return null;

            var sb = new StringBuilder();
            for (int i = 0; i < schema.Enum.Count; i++)
            {
                var value = ((OpenApiInteger)schema.Enum[i]).Value;
                var name = ((OpenApiString)((OpenApiArray)schema.Extensions["x-enumNames"])[i]).Value;
                sb.Append($"<br>{value} = {name}");

                // add description from DescriptionAttribute
                if (includeDescriptionFromAttribute)
                {
                    if (!schema.Extensions.ContainsKey("x-enumDescriptions"))
                        continue;

                    var xenumDescriptions = (OpenApiArray)schema.Extensions["x-enumDescriptions"];
                    if (xenumDescriptions?.Count == schema.Enum.Count)
                    {
                        var description = ((OpenApiString)((OpenApiArray)schema.Extensions["x-enumDescriptions"])[i]).Value;
                        sb.Append($" ({description})");
                    }
                }
            }
            return sb.ToString();
        }
    }
}
