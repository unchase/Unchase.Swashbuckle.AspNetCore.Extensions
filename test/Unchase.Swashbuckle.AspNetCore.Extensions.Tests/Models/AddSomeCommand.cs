namespace Unchase.Swashbuckle.AspNetCore.Extensions.Tests.Models
{
    /// <summary>
    /// CommandType
    /// </summary>
    public class CommandType
    {
        /// <summary>
        /// Name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// External Id.
        /// </summary>
        public string ExternalId { get; set; }

        /// <summary>
        /// Is Read Only.
        /// </summary>
        /// <remarks>
        /// Is Read Only remarks.
        /// </remarks>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Is Active.
        /// </summary>
        /// <remarks>
        /// Is Active remarks.
        /// </remarks>
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// AddSomeCommand.
    /// </summary>
    public class AddSomeCommand
    {
        /// <inheritdoc cref="CommandType.Name"/>
        /// <example>Example</example>
        public string Name { get; set; }

        /// <inheritdoc cref="CommandType.Description"/>
        /// <example>Example</example>
        public string Description { get; set; }

        /// <inheritdoc cref="CommandType.ExternalId"/>
        /// <example>Example</example>
        public string ExternalId { get; set; }

        /// <inheritdoc cref="CommandType.IsReadOnly"/>
        /// <example>false</example>
        public bool IsReadOnly { get; set; }

        /// <inheritdoc cref="CommandType.IsActive"/>
        /// <example>true</example>
        public bool IsActive { get; set; }
    }
}