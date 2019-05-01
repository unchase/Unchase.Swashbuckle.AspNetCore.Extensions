using System.ComponentModel;
using System.Runtime.Serialization;

namespace WebApi2._0_Swashbuckle.Models
{
    /// <summary>
    /// Title enum.
    /// </summary>
    [DataContract]
    public enum Title
    {
        /// <summary>
        /// None.
        /// </summary>
        [Description("None enum description")]
        [EnumMember]
        None = 0,

        /// <summary>
        /// Miss.
        /// </summary>
        [Description("Miss enum description")]
        [EnumMember]
        Miss,

        /// <summary>
        /// Mr.
        /// </summary>
        [Description("Mr enum description")]
        [EnumMember]
        Mr
    }
}
