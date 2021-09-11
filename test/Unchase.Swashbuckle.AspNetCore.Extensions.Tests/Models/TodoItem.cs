using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Tests.Models
{
    /// <summary>
    /// Todo item.
    /// </summary>
    /// <remarks>
    /// Todo item remarks - class
    /// </remarks>
    public interface ITodoItem : ITodoItemName
    {
        /// <summary>
        /// Id
        /// </summary>
        /// <remarks>
        /// Unique identifier - parameter
        /// </remarks>
        long Id { get; set; }
    }

    /// <summary>
    /// Interface with name.
    /// </summary>
    public interface ITodoItemName
    {
        /// <summary>
        /// Name
        /// </summary>
        /// <remarks>
        /// Name of todo item - parameter
        /// </remarks>
        string Name { get; set; }
    }

    /// <inheritdoc/>
    public class TodoItem : ITodoItem
    {
        /// <inheritdoc/>
        public long Id { get; set; }

        /// <inheritdoc/>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Is complete
        /// </summary>
        /// <remarks>
        /// The todo item is completed - parameter
        /// </remarks>
        [DefaultValue(false)]
        public bool IsComplete { get; set; }

        /// <summary>
        /// Tag
        /// </summary>
        /// <remarks>
        /// Todo item tag - parameter
        /// </remarks>
        [Description("Todo item tag - description")]
        public Tag Tag { get; set; }
    }
}
