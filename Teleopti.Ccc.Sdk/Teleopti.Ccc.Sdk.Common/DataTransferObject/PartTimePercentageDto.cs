using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// A part time percentage detail.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class PartTimePercentageDto : Dto
    {
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the percentage. The form for the percentage is as 1.0 = 100%, 0.75 = 75% etc.
        /// </summary>
        /// <value>The percentage.</value>
        [DataMember]
        public double Percentage { get; set; }

        /// <summary>
        /// Gets or sets the delete flag.
        /// </summary>
        /// <remarks>Indicates whether this part time percentage should not be used anymore.</remarks>
        [DataMember(IsRequired = false,Order = 1)]
        public bool IsDeleted { get; set; }
    }
}
