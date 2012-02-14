using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Definition set for overtime (must be included when creating new overtime layers)
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class OvertimeDefinitionSetDto : Dto
    {
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the deleted flag.
        /// </summary>
        /// <remarks>Indicates whether this overtime definition set should not be used anymore.</remarks>
        [DataMember(IsRequired = false,Order = 1)]
        public bool IsDeleted { get; set; }
    }
}
