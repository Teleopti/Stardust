using System.Runtime.Serialization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents a ShiftCategoryDto object.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class ShiftCategoryDto : Dto
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [DataMember]
        public string Name{ get; set; }

        /// <summary>
        /// Gets or sets the short name.
        /// </summary>
        /// <value>The short name.</value>
        [DataMember]
        public string ShortName{ get; set; }

        /// <summary>
        /// Gets or sets the display color.
        /// </summary>
        /// <value>The display color.</value>
        [DataMember]
        public ColorDto DisplayColor { get; set; } 
    }
}