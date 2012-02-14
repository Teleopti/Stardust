using System.Runtime.Serialization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Class representing forcasted vs scheduled data
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class SkillDataDto
    {
        /// <summary>
        /// Gets or sets the forecasted agents.
        /// </summary>
        /// <value>The forecasted agents.</value>
        [DataMember]
        public double ForecastedAgents { get; set; }

        /// <summary>
        /// Gets or sets the scheduled agents.
        /// </summary>
        /// <value>The scheduled agents.</value>
        [DataMember]
        public double ScheduledAgents { get; set; }

        /// <summary>
        /// Gets or sets the scheduled heads, the number of actual persons.
        /// </summary>
        /// <value>The scheduled heads.</value>
        [DataMember]
        public double ScheduledHeads { get; set; }

        /// <summary>
        /// Gets or sets the intra interval standard deviation.
        /// </summary>
        /// <value>The interval standard deviation.</value>
        [DataMember]
        public double IntervalStandardDeviation { get; set; }

        /// <summary>
        /// Gets or sets the estimated service level.
        /// </summary>
        /// <value>The estimated service level.</value>
        [DataMember]
        public double EstimatedServiceLevel { get; set; }

        /// <summary>
        /// Gets or sets the period.
        /// </summary>
        /// <value>The period.</value>
        [DataMember]
        public DateTimePeriodDto Period { get; set; }
    }
}