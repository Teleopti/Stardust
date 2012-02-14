using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Details for the main shift.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class MainShiftDto:ShiftDto
    {
        /// <summary>
        /// Gets or sets the id of the shift category.
        /// </summary>
        [DataMember]
        public Guid ShiftCategoryId { get; set; }

        /// <summary>
        /// Gets or sets the name of the shift category.
        /// </summary>
        [DataMember]
        public string ShiftCategoryName { get; set; }

        /// <summary>
        /// Gets or sets the short name of the shift category.
        /// </summary>
        [DataMember]
        public string ShiftCategoryShortName { get; set; }

    }
}