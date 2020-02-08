using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using WebApi3._1_Swashbuckle.Models;

namespace TodoApi.Models
{
    /// <summary>
    /// Todo item.
    /// </summary>
    public class TodoItem
    {
        /// <summary>
        /// Id.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Name.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Is complete.
        /// </summary>
        [DefaultValue(false)]
        public bool IsComplete { get; set; }

        /// <summary>
        /// Tag.
        /// </summary>
        [Description("Todo item tag")]
        public Tag Tag { get; set; }
    }
}
