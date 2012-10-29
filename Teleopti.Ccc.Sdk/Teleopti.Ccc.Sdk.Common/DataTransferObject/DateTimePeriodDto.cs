using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents a DateTimePeriodDto object.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    [Serializable]
    public class DateTimePeriodDto : Dto
    {
        /// <summary>
        /// Gets or sets the UTC start time.
        /// </summary>
        /// <value>The UTC start time.</value>
        [DataMember]
        public DateTime UtcStartTime { get; set; }

        /// <summary>
        /// Gets or sets the UTC end time.
        /// </summary>
        /// <value>The UTC end time.</value>
        [DataMember]
        public DateTime UtcEndTime { get; set; }

        /// <summary>
        /// Gets or sets the local start date time.
        /// </summary>
        /// <value>The local start date time.</value>
        [DataMember]
        public DateTime LocalStartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the local end date time.
        /// </summary>
        /// <value>The local end date time.</value>
        [DataMember]
        public DateTime LocalEndDateTime { get; set; }
    }
}