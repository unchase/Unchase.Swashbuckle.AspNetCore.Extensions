using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Filters
{
    /// <summary>
    /// Document filter for appending action count to Tag summary of controllers.
    /// </summary>
    public class AppendActionCountToTagSummaryDocumentFilter : IDocumentFilter
    {
        private readonly string _messageTemplate;

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public AppendActionCountToTagSummaryDocumentFilter(string messageTemplate = "(action count: {0})")
        {
            _messageTemplate = messageTemplate;
            if (!_messageTemplate.Contains("{0}"))
            {
                throw new ArgumentException("The message template must contains '{0}'.", nameof(messageTemplate));
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Apply filter.
        /// </summary>
        /// <param name="openApiDoc"><see cref="OpenApiDocument"/></param>
        /// <param name="context"><see cref="DocumentFilterContext"/></param>
        public void Apply(OpenApiDocument openApiDoc, DocumentFilterContext context)
        {
            if (openApiDoc.Tags == null)
            {
                return;
            }

            var tagActionCount = new Dictionary<string, int>();
            foreach (var path in openApiDoc.Paths)
            {
                var possibleParameterizedOperations = path.Value.Operations.Select(o => o.Value);
                possibleParameterizedOperations.Where(o => o?.Tags != null).ToList().ForEach(o =>
                {
                    foreach (var tag in o.Tags)
                    {
                        if (!tagActionCount.ContainsKey(tag.Name))
                        {
                            tagActionCount.Add(tag.Name, 1);
                        }
                        else
                        {
                            tagActionCount[tag.Name]++;
                        }
                    }
                });
            }

            foreach (var tagActionCountKey in tagActionCount.Keys)
            {
                foreach (var tag in openApiDoc.Tags)
                {
                    if (tag.Name == tagActionCountKey)
                    {
                        tag.Description += string.Format($" {_messageTemplate}", tagActionCount[tagActionCountKey]);
                    }
                }
            }
        }

        #endregion
    }
}