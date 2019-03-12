using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents a DateTimePeriodDto object.
    /// </summary>
	[DebuggerDisplay("UTC: {UtcStartTime} -> {UtcEndTime}  |  Local: {LocalStartDateTime} -> {LocalEndDateTime}")]
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    [Serializable]
    public class DateTimePeriodDto : Dto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimePeriodDto"/> class.
        /// </summary>
        /// <param name="period">The period.</param>
        [Obsolete("This constructor should not be used. Use empty constructor and set datetimes if you need a new instance, or use the dedicated assembler.")]
        public DateTimePeriodDto(DateTimePeriod period) : this()
        {
            UtcStartTime = period.StartDateTime;
            UtcEndTime = period.EndDateTime;
        }

		public DateTimePeriodDto()
		{
		}

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