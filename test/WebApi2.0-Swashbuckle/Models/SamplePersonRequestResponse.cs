using System.ComponentModel;

namespace WebApi2._0_Swashbuckle.Models
{
    /// <summary>
    /// Sample Person request and response.
    /// </summary>
    public class SamplePersonRequestResponse
    {
        /// <summary>
        /// Sample Person title.
        /// </summary>
        public Title Title { get; set; }

        /// <summary>
        /// Sample Person age.
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// Sample Person firstname.
        /// </summary>
        [Description("The first name of the person")]
        public string FirstName { get; set; }

        /// <summary>
        /// Sample Person income.
        /// </summary>
        public decimal? Income { get; set; }
    }
}
