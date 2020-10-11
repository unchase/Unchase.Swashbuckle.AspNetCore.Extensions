using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Factories
{
    /// <summary>
    /// <see cref="ModelMetadata"/> factory.
    /// </summary>
    public static class ModelMetadataFactory
    {
        #region Methods

        /// <summary>
        /// Create <see cref="ModelMetadata"/> for type.
        /// </summary>
        /// <param name="type">Type of method.</param>
        public static ModelMetadata CreateForType(Type type)
        {
            return new EmptyModelMetadataProvider().GetMetadataForType(type);
        }

        /// <summary>
        /// Create <see cref="ModelMetadata"/> for property.
        /// </summary>
        /// <param name="containingType">Type of property method.</param>
        /// <param name="propertyName">Name of property.</param>
        public static ModelMetadata CreateForProperty(Type containingType, string propertyName)
        {
            return new EmptyModelMetadataProvider().GetMetadataForProperty(containingType, propertyName);
        }

        /// <summary>
        /// Create <see cref="ModelMetadata"/> for parameter.
        /// </summary>
        /// <param name="parameter"><see cref="ParameterInfo"/></param>
        public static ModelMetadata CreateForParameter(ParameterInfo parameter)
        {
            return new EmptyModelMetadataProvider().GetMetadataForParameter(parameter);
        }

        #endregion
    }
}