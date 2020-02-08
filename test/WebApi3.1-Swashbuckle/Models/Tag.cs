using System.ComponentModel;

namespace WebApi3._1_Swashbuckle.Models
{
    /// <summary>
    /// Tag for TodoItem.
    /// </summary>
    [Description("Tag enum")]
    public enum Tag
    {
        /// <summary>
        /// None.
        /// </summary>
        [Description("Default tag")]
        None = 0,

        /// <summary>
        /// Task.
        /// </summary>
        [Description("Some task")]
        Task = 1,

        /// <summary>
        /// Workout.
        /// </summary>
        [Description("Periodical job")]
        Workout = 2
    }
}
