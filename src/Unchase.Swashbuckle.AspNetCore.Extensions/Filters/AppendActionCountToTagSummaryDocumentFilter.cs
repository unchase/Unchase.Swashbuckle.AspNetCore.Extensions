using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Filters
{
    /// <summary>
    /// Document filter for appending action count to Tag summary of controllers.
    /// </summary>
    public class AppendActionCountToTagSummaryDocumentFilter : IDocumentFilter
    {
        #region Methods

        /// <summary>
        /// Apply filter.
        /// </summary>
        /// <param name="openApiDoc"><see cref="OpenApiDocument"/></param>
        /// <param name="context"><see cref="DocumentFilterContext"/></param>
        public void Apply(OpenApiDocument openApiDoc, DocumentFilterContext context)
        {
            if (openApiDoc.Tags == null)
                return;

            var tagActionCount = new Dictionary<string, int>();
            foreach (var path in openApiDoc.Paths)
            {
                var possibleParameterizedOperations = path.Value.Operations.Select(o => o.Value);
                possibleParameterizedOperations.Where(o => o?.Tags != null).ToList().ForEach(o =>
                {
                    foreach (var tag in o.Tags)
                    {
                        if (!tagActionCount.ContainsKey(tag.Name))
                            tagActionCount.Add(tag.Name, 1);
                        else
                            tagActionCount[tag.Name]++;
                    }
                });
            }

            foreach (var tagActionCountKey in tagActionCount.Keys)
            {
                foreach (var tag in openApiDoc.Tags)
                {
                    if (tag.Name == tagActionCountKey)
                        tag.Description += $" (action count: {tagActionCount[tagActionCountKey]})";
                }
            }
        }

        #endregion
    }
}
