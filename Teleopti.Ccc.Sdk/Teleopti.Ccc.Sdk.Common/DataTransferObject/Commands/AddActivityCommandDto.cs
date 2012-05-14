using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
    /// <summary>
    /// This command adds an activity layer to a schedule. The activity layer will be created according to a specified <see cref="ActivityId"/> and <see cref="Period"/>. You can specify the schedule by a person's <see cref="PersonId"/> and the <see cref="Date"/> of schedule. Note that, the total assignment length after the activity is added should be less than 36 hours. 
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/10/")]
    public class AddActivityCommandDto : CommandDto
    {
        /// <summary>
        /// Gets or sets the person Id.
        /// </summary>
        /// <value>The person Id.</value>
        [DataMember]
        public Guid PersonId { get; set; }

        /// <summary>
        /// Gets or sets the date of schedule.
        /// </summary>
        /// <value>The date of schedule.</value>
        [DataMember]
        public DateOnlyDto Date { get; set; }

        /// <summary>
        /// Gets or sets the period of the activity.
        /// </summary>
        /// <value>The period.</value>
        [DataMember]
        public DateTimePeriodDto Period { get; set; }

        /// <summary>
        /// Gets or sets the activity Id.
        /// </summary>
        /// <value>The activity Id.</value>
        [DataMember]
        public Guid ActivityId { get; set; }

		/// <summary>
		/// Gets or sets the scenario id. If omitted default scenario will be used.
		/// </summary>
		[DataMember(Order = 1, IsRequired = false)]
    	public Guid? ScenarioId { get; set; }
    }
}
