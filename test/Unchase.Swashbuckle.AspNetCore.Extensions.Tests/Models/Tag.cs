using System.ComponentModel;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Tests.Models
{
    /// <summary>
    /// Tag for TodoItem
    /// </summary>
    /// <remarks>
    /// Tag for TodoItem remarks - enum
    /// </remarks>
    [Description("Tag enum - description")]
    public enum Tag
    {
        /// <summary>
        /// None
        /// </summary>
        /// <remarks>
        /// None tag remarks
        /// </remarks>
        [Description("Default tag - description")]
        None = 0,

        /// <summary>
        /// Task
        /// </summary>
        /// <remarks>
        /// Task tag remarks
        /// </remarks>
        [Description("Some task - description")]
        Task = 1,

        /// <summary>
        /// Workout
        /// </summary>
        /// <remarks>
        /// Workout tag remarks
        /// </remarks>
        [Description("Periodical job - description")]
        Workout = 2
    }
}
