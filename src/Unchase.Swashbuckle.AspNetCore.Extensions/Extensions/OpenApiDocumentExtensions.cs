using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Unchase.Swashbuckle.AspNetCore.Extensions.Factories;
using Unchase.Swashbuckle.AspNetCore.Extensions.Filters;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Extensions
{
    /// <summary>
    /// Extension methods for <see cref=" OpenApiDocument"/>.
    /// </summary>
    public static class OpenApiDocumentExtensions
    {
        /// <summary>
        /// Remove Paths and Components from OpenApi documentation for specific controller action without accepted roles.
        /// </summary>
        /// <param name="openApiDoc"><see cref="OpenApiDocument"/>.</param>
        /// <param name="actionNameSelector">Action name selector.</param>
        /// <param name="acceptedRoles">Collection of accepted roles.</param>
        /// <returns>
        /// Returns <see cref="OpenApiDocument"/>.
        /// </returns>
        public static OpenApiDocument RemovePathsAndComponentsWithoutAcceptedRolesFor<TController>(this OpenApiDocument openApiDoc, Func<TController, string> actionNameSelector,
            IReadOnlyList<string> acceptedRoles) where TController : class, new()
        {
            var actionDescriptor = ApiDescriptionFactory.Create(actionNameSelector, typeof(TController).GetCustomAttribute<RouteAttribute>().Template)?.ActionDescriptor;
            if (actionDescriptor != null)
            {
                var paths = new Dictionary<MethodInfo, string>
                {
                    { ((Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)actionDescriptor).MethodInfo, actionDescriptor.AttributeRouteInfo.Template }
                };

                HidePathsAndDefinitionsByRolesDocumentFilter.RemovePathsAndComponents(openApiDoc, paths, openApiDoc.Components.Schemas, acceptedRoles);
            }

            return openApiDoc;
        }

        /// <summary>
        /// Remove Paths and Components from OpenApi documentation for specific controller without accepted roles.
        /// </summary>
        /// <param name="openApiDoc"><see cref="OpenApiDocument"/>.</param>
        /// <param name="acceptedRoles">Collection of accepted roles.</param>
        /// <returns>
        /// Returns <see cref="OpenApiDocument"/>.
        /// </returns>
        public static OpenApiDocument RemovePathsAndComponentsWithoutAcceptedRolesForController<TController>(this OpenApiDocument openApiDoc,
            IReadOnlyList<string> acceptedRoles) where TController : class
        {
            var paths = new Dictionary<MethodInfo, string>();
            foreach (var methodInfo in typeof(TController).GetMethods().Where(m => !m.IsSpecialName))
            {
                var actionDescriptor = ApiDescriptionFactory.Create<TController>(methodInfo, typeof(TController).GetCustomAttribute<RouteAttribute>().Template)?.ActionDescriptor;
                if (actionDescriptor != null)
                {
                    paths.Add(((Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)actionDescriptor).MethodInfo, actionDescriptor.AttributeRouteInfo.Template);
                }
            }
            HidePathsAndDefinitionsByRolesDocumentFilter.RemovePathsAndComponents(openApiDoc, paths, openApiDoc.Components.Schemas, acceptedRoles);

            return openApiDoc;
        }
    }
}
