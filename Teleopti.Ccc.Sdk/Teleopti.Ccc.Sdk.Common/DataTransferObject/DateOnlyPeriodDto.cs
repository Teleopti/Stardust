
using System;
using System.Runtime.Serialization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents a DateOnlyPeriodDto object.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    [Serializable]
    public class DateOnlyPeriodDto :Dto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateOnlyPeriodDto"/> class.
        /// </summary>
        public DateOnlyPeriodDto()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateOnlyPeriodDto"/> class.
        /// </summary>
        /// <param name="period">The period.</param>
        public DateOnlyPeriodDto(DateOnlyPeriod period  )
        {
            StartDate = new DateOnlyDto(period.StartDate);
            EndDate = new DateOnlyDto(period.EndDate);
        }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>The start date.</value>
        [DataMember]
        public DateOnlyDto StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>The end date.</value>
        [DataMember]
        public DateOnlyDto EndDate { get; set; }
    }
}