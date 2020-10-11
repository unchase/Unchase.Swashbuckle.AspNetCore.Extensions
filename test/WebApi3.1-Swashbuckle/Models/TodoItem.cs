using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using WebApi3._1_Swashbuckle.Models;

namespace TodoApi.Models
{
    /// <summary>
    /// Todo item.
    /// </summary>
    /// <remarks>
    /// Todo item remarks - class
    /// </remarks>
    public class TodoItem
    {
        /// <summary>
        /// Id
        /// </summary>
        /// <remarks>
        /// Unique identifier - parameter
        /// </remarks>
        public long Id { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        /// <remarks>
        /// Name of todo item - parameter
        /// </remarks>
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
