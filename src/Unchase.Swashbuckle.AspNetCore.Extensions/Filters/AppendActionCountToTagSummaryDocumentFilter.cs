using System.Collections.Generic;
using System.Linq;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Filters
{
    public class AppendActionCountToTagSummaryDocumentFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            if (swaggerDoc.Tags == null)
                return;

            var tagActionCount = new Dictionary<string, int>();
            foreach (var path in swaggerDoc.Paths)
            {
                var possibleParameterizedOperations = new List<Operation> {path.Value.Get, path.Value.Post, path.Value.Put, path.Value.Delete, path.Value.Patch};
                possibleParameterizedOperations.Where(o => o?.Tags != null).ToList().ForEach(o =>
                {
                    foreach (var tag in o.Tags)
                    {
                        if (!tagActionCount.ContainsKey(tag))
                            tagActionCount.Add(tag, 1);
                        else
                            tagActionCount[tag]++;
                    }
                });
            }

            foreach (var tagActionCountKey in tagActionCount.Keys)
            {
                foreach (var tag in swaggerDoc.Tags)
                {
                    if (tag.Name == tagActionCountKey)
                        tag.Description += $" (action count: {tagActionCount[tagActionCountKey]})";
                }
            }
        }
    }
}
