using System;
using System.Runtime.Serialization;
using Teleopti.Ccc.Sdk.Common.Contracts;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
	/// <summary>
	/// This command adds a layer of personal activity to a schedule. The personal activity layer will be created according to a specified <see cref="ActivityId"/> and <see cref="Period"/>. You can specify the schedule by a person's <see cref="PersonId"/> and the <see cref="Date"/> of schedule. Note that, the total assignment length after the personal activity is added should be less than 36 hours. 
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2012/04/")]
	public class AddPersonalActivityCommandDto : CommandDto
	{
		/// <summary>
		/// Gets or sets the mandatory person Id.
		/// </summary>
		/// <value>The person Id.</value>
		[DataMember]
		public Guid PersonId { get; set; }

		/// <summary>
		/// Gets or sets the mandatory start date for the target shift.
		/// </summary>
		/// <value>The target date.</value>
		[DataMember]
		public DateOnlyDto Date { get; set; }

		/// <summary>
		/// Gets or sets the mandatory period for this activity.
		/// </summary>
		/// <value>The period.</value>
		[DataMember]
		public DateTimePeriodDto Period { get; set; }

		/// <summary>
		/// Gets or sets the activity id. Ususally comes from an <see cref="ActivityDto"/> loaded using the <see cref="ITeleoptiSchedulingService.GetActivities"/> method.
		/// </summary>
		/// <value>The activity Id.</value>
		[DataMember]
		public Guid ActivityId { get; set; }

		/// <summary>
		/// Gets or sets the scenario id. If omitted default scenario will be used.
		/// </summary>
		[DataMember]
		public Guid? ScenarioId { get; set; }

        /// <summary>
        /// Gets or sets the schedule tag id. If omitted then the current schedule tag will be kept.
        /// </summary>
        [DataMember(Order = 2, IsRequired = false)]
        public Guid? ScheduleTagId { get; set; }
	}
}