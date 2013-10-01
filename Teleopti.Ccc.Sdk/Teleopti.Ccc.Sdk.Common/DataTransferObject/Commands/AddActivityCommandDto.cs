using System;
using System.Runtime.Serialization;
using Teleopti.Ccc.Sdk.Common.Contracts;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
    /// <summary>
    /// This command adds an activity layer to a schedule. The activity layer will be created according to a specified <see cref="ActivityId"/> and <see cref="Period"/>. You can specify the schedule by a person's <see cref="PersonId"/> and the <see cref="Date"/> of schedule. Note that, the total assignment length after the activity is added should be less than 36 hours. 
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/10/")]
    public class AddActivityCommandDto : CommandDto
    {
        /// <summary>
        /// Gets or sets the mandatory person Id.
        /// </summary>
        /// <value>The person Id.</value>
        [DataMember]
        public Guid PersonId { get; set; }

        /// <summary>
        /// Gets or sets the mandatory date of schedule (the day that the target shift starts).
        /// </summary>
        /// <value>The date of schedule.</value>
        [DataMember]
        public DateOnlyDto Date { get; set; }

        /// <summary>
        /// Gets or sets the mandatory period of the activity.
        /// </summary>
        /// <value>The period.</value>
        [DataMember]
        public DateTimePeriodDto Period { get; set; }

        /// <summary>
        /// Gets or sets the mandatory activity id. Usually comes from an <see cref="ActivityDto"/> loaded using the <see cref="ITeleoptiSchedulingService.GetActivities"/> method.
        /// </summary>
        /// <value>The activity Id.</value>
        [DataMember]
        public Guid ActivityId { get; set; }

		/// <summary>
		/// Gets or sets the scenario id. If omitted default scenario will be used.
		/// </summary>
		[DataMember(Order = 1, IsRequired = false)]
    	public Guid? ScenarioId { get; set; }

        /// <summary>
        /// Gets or sets the schedule tag id. If omitted then Null schedule tag will be used.
        /// </summary>
        [DataMember(Order = 2, IsRequired = false)]
        public Guid? ScheduleTagId { get; set; }
    }
}
