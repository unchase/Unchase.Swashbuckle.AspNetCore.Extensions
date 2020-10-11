using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Factories
{
    /// <summary>
    /// <see cref="ApiDescription"/> factory.
    /// </summary>
    internal static class ApiDescriptionFactory
    {
        #region Methods

        /// <summary>
        /// Create <see cref="ApiDescription"/>.
        /// </summary>
        /// <param name="controllerType">Type of Controller.</param>
        /// <param name="methodInfo">Controller action <see cref="MethodInfo"/>.</param>
        /// <param name="groupName">Group name.</param>
        /// <param name="httpMethod">Http method.</param>
        /// <param name="relativePath">Relative path.</param>
        /// <param name="parameterDescriptions">Collection of <see cref="ApiParameterDescription"/>.</param>
        /// <param name="supportedRequestFormats">Collection of <see cref="ApiRequestFormat"/>.</param>
        /// <param name="supportedResponseTypes">Collection of <see cref="ApiResponseType"/>.</param>
        /// <returns>
        /// Returns <see cref="ApiDescription"/>.
        /// </returns>
        internal static ApiDescription Create(
            Type controllerType,
            MethodInfo methodInfo,
            string groupName = null,
            string httpMethod = null,
            string relativePath = null,
            IEnumerable<ApiParameterDescription> parameterDescriptions = null,
            IEnumerable<ApiRequestFormat> supportedRequestFormats = null,
            IEnumerable<ApiResponseType> supportedResponseTypes = null)
        {
            if (methodInfo == null)
                return null;

            var actionDescriptor = CreateActionDescriptor(methodInfo);

            var routAttr = controllerType.GetCustomAttributes(true).OfType<RouteAttribute>().LastOrDefault();

            if (string.IsNullOrWhiteSpace(actionDescriptor?.AttributeRouteInfo?.Template))
                return null;
            //throw new InvalidOperationException($"HttpMethod attribute of \"{methodInfo.Name}\" action in \"{controllerType.Name}\" controller must have a template specified.");

            if (routAttr != null && !string.IsNullOrWhiteSpace(routAttr.Template))
            {
                var template = routAttr.Template;
                actionDescriptor.AttributeRouteInfo.Template = template + "/" + actionDescriptor.AttributeRouteInfo.Template;
            }

            foreach (var routeValue in actionDescriptor.RouteValues)
            {
                actionDescriptor.AttributeRouteInfo.Template =
                    actionDescriptor.AttributeRouteInfo.Template.Replace($"[{routeValue.Key}]", routeValue.Value);
            }

            httpMethod = httpMethod ?? methodInfo?.GetCustomAttributes(true).OfType<HttpMethodAttribute>().FirstOrDefault()?.HttpMethods?.FirstOrDefault();

            var apiDescription = new ApiDescription
            {
                ActionDescriptor = actionDescriptor,
                GroupName = groupName,
                HttpMethod = httpMethod,
                RelativePath = relativePath,
            };

            if (parameterDescriptions != null)
            {
                foreach (var parameter in parameterDescriptions)
                {
                    // If the provided action has a matching parameter - use it to assign ParameterDescriptor & ModelMetadata
                    var controllerParameterDescriptor = actionDescriptor.Parameters
                        .OfType<ControllerParameterDescriptor>()
                        .FirstOrDefault(parameterDescriptor => parameterDescriptor.Name == parameter.Name);

                    if (controllerParameterDescriptor != null)
                    {
                        parameter.ParameterDescriptor = controllerParameterDescriptor;
                        parameter.ModelMetadata = ModelMetadataFactory.CreateForParameter(controllerParameterDescriptor.ParameterInfo);
                    }

                    apiDescription.ParameterDescriptions.Add(parameter);
                }
            }

            if (supportedRequestFormats != null)
            {
                foreach (var requestFormat in supportedRequestFormats)
                {
                    apiDescription.SupportedRequestFormats.Add(requestFormat);
                }
            }

            if (supportedResponseTypes != null)
            {
                foreach (var responseType in supportedResponseTypes)
                {
                    // If the provided action has a return value AND the response status is 2XX - use it to assign ModelMetadata
                    if (methodInfo?.ReturnType != null && responseType.StatusCode / 100 == 2)
                    {
                        responseType.ModelMetadata = ModelMetadataFactory.CreateForType(methodInfo.ReturnType);
                    }

                    apiDescription.SupportedResponseTypes.Add(responseType);
                }
            }

            return apiDescription;
        }

        /// <summary>
        /// Create <see cref="ApiDescription"/>.
        /// </summary>
        /// <param name="controllerType">Type of Controller.</param>
        /// <param name="actionName">Controller action name.</param>
        /// <param name="groupName">Group name.</param>
        /// <param name="httpMethod">Http method.</param>
        /// <param name="relativePath">Relative path.</param>
        /// <param name="parameterDescriptions">Collection of <see cref="ApiParameterDescription"/>.</param>
        /// <param name="supportedRequestFormats">Collection of <see cref="ApiRequestFormat"/>.</param>
        /// <param name="supportedResponseTypes">Collection of <see cref="ApiResponseType"/>.</param>
        /// <returns>
        /// Returns <see cref="ApiDescription"/>.
        /// </returns>
        internal static ApiDescription Create(
            Type controllerType,
            string actionName,
            string groupName = null,
            string httpMethod = null,
            string relativePath = null,
            IEnumerable<ApiParameterDescription> parameterDescriptions = null,
            IEnumerable<ApiRequestFormat> supportedRequestFormats = null,
            IEnumerable<ApiResponseType> supportedResponseTypes = null)
        {
            MethodInfo methodInfo;

            try
            {
                methodInfo = controllerType.GetMethod(actionName);
            }
            catch (AmbiguousMatchException)
            {
                return null;
            }

            return Create(controllerType, methodInfo, groupName, httpMethod, relativePath, parameterDescriptions,
                supportedRequestFormats, supportedResponseTypes);
        }

        /// <summary>
        /// Create <see cref="ApiDescription"/>.
        /// </summary>
        /// <typeparam name="TController">Type of Controller.</typeparam>
        /// <param name="actionNameSelector">Action name selector.</param>
        /// <param name="groupName">Group name.</param>
        /// <param name="httpMethod">Http method.</param>
        /// <param name="relativePath">Relative path.</param>
        /// <param name="parameterDescriptions">Collection of <see cref="ApiParameterDescription"/>.</param>
        /// <param name="supportedRequestFormats">Collection of <see cref="ApiRequestFormat"/>.</param>
        /// <param name="supportedResponseTypes">Collection of <see cref="ApiResponseType"/>.</param>
        /// <returns>
        /// Returns <see cref="ApiDescription"/>.
        /// </returns>
        internal static ApiDescription Create<TController>(
            Func<TController, string> actionNameSelector,
            string groupName = null,
            string httpMethod = null,
            string relativePath = null,
            IEnumerable<ApiParameterDescription> parameterDescriptions = null,
            IEnumerable<ApiRequestFormat> supportedRequestFormats = null,
            IEnumerable<ApiResponseType> supportedResponseTypes = null)
            where TController : new()
        {
            return Create(
                typeof(TController),
                actionNameSelector(new TController()),
                groupName,
                httpMethod,
                relativePath,
                parameterDescriptions,
                supportedRequestFormats,
                supportedResponseTypes
            );
        }

        /// <summary>
        /// Create <see cref="ApiDescription"/>.
        /// </summary>
        /// <typeparam name="TController">Type of Controller.</typeparam>
        /// <param name="methodInfo">Action <see cref="MethodInfo"/>.</param>
        /// <param name="groupName">Group name.</param>
        /// <param name="httpMethod">Http method.</param>
        /// <param name="relativePath">Relative path.</param>
        /// <param name="parameterDescriptions">Collection of <see cref="ApiParameterDescription"/>.</param>
        /// <param name="supportedRequestFormats">Collection of <see cref="ApiRequestFormat"/>.</param>
        /// <param name="supportedResponseTypes">Collection of <see cref="ApiResponseType"/>.</param>
        /// <returns>
        /// Returns <see cref="ApiDescription"/>.
        /// </returns>
        internal static ApiDescription Create<TController>(
            MethodInfo methodInfo,
            string groupName = null,
            string httpMethod = null,
            string relativePath = null,
            IEnumerable<ApiParameterDescription> parameterDescriptions = null,
            IEnumerable<ApiRequestFormat> supportedRequestFormats = null,
            IEnumerable<ApiResponseType> supportedResponseTypes = null)
            where TController : class
        {
            return Create(
                typeof(TController),
                methodInfo,
                groupName,
                httpMethod,
                relativePath,
                parameterDescriptions,
                supportedRequestFormats,
                supportedResponseTypes
            );
        }

        private static ActionDescriptor CreateActionDescriptor(MethodInfo methodInfo)
        {
            var httpMethodAttribute = methodInfo.GetCustomAttribute<HttpMethodAttribute>();
            var attributeRouteInfo = (httpMethodAttribute != null)
                ? new AttributeRouteInfo { Template = httpMethodAttribute.Template, Name = httpMethodAttribute.Name }
                : null;

            var parameterDescriptors = methodInfo.GetParameters()
                .Select(CreateParameterDescriptor)
                .ToList();

            var routeValues = new Dictionary<string, string>
            {
                ["controller"] = methodInfo.DeclaringType?.Name.Replace("Controller", string.Empty)
            };

            return new ControllerActionDescriptor
            {
                AttributeRouteInfo = attributeRouteInfo,
                ControllerTypeInfo = methodInfo.DeclaringType.GetTypeInfo(),
                MethodInfo = methodInfo,
                Parameters = parameterDescriptors,
                RouteValues = routeValues
            };
        }

        private static ParameterDescriptor CreateParameterDescriptor(ParameterInfo parameterInfo)
        {
            return new ControllerParameterDescriptor
            {
                Name = parameterInfo.Name,
                ParameterInfo = parameterInfo,
                ParameterType = parameterInfo.ParameterType,
            };
        }

        #endregion
    }
}
