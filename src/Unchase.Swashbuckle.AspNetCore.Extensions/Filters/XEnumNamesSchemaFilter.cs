using System;
using System.Linq;
using System.Reflection;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Filters
{
    public class XEnumNamesSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema model, SchemaFilterContext context)
        {
            var typeInfo = context.SystemType.GetTypeInfo();
            if (typeInfo.IsEnum)
            {
                var names = Enum.GetNames(context.SystemType);
                model.Extensions.Add("x-enumNames", names);
            }
            else if (model.Enum != null)
            {
                var names = model.Enum.Select(n => n.ToString());
                model.Extensions.Add("x-enumNames", names);
            }
            else if (typeInfo.IsGenericType && !model.Extensions.ContainsKey("x-enumNames"))
            {
                foreach (var genericArgumentType in typeInfo.GetGenericArguments())
                {
                    if (genericArgumentType.IsEnum)
                    {
                        var names = Enum.GetNames(genericArgumentType);
                        if (!model.Extensions.ContainsKey("x-enumNames"))
                            model.Extensions.Add("x-enumNames", names);
                    }
                }
            }
        }
    }
}
