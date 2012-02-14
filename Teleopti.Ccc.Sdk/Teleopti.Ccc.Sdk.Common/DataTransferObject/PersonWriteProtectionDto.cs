using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Write protection information for one person.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class PersonWriteProtectionDto
    {
        /// <summary>
        /// Gets or sets the person id.
        /// </summary>
        [DataMember]
         public Guid PersonId { get; set; }

        /// <summary>
        /// Gets or sets the date schedule is write protected until.
        /// </summary>
        [DataMember]
        public DateOnlyDto WriteProtectedToDate { get; set; }

    }
}