using System;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Filters
{
    public class XEnumNamesParameterFilter : IParameterFilter
    {
        public void Apply(IParameter parameter, ParameterFilterContext context)
        {
            var typeInfo = context.ParameterInfo?.ParameterType?? context.PropertyInfo.PropertyType;
            if (typeInfo.IsEnum)
            {
                var names = Enum.GetNames(typeInfo);
                parameter.Extensions.Add("x-enumNames", names);
            }
            else if (typeInfo.IsGenericType && !parameter.Extensions.ContainsKey("x-enumNames"))
            {
                foreach (var genericArgumentType in typeInfo.GetGenericArguments())
                {
                    if (genericArgumentType.IsEnum)
                    {
                        var names = Enum.GetNames(genericArgumentType);
                        if (!parameter.Extensions.ContainsKey("x-enumNames"))
                            parameter.Extensions.Add("x-enumNames", names);
                    }
                }
            }
        }
    }
}
