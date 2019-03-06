using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
	/// <summary>
	/// This command cancels all personal activities within a specified <see cref="Period"/>. All personal activities that intersect with the specified <see cref="Period"/> will be hollowed out. 
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2012/04/")]
	public class CancelPersonalActivityCommandDto : CommandDto
	{
		/// <summary>
		/// Gets or sets the mandatory person Id.
		/// </summary>
		/// <value>The person Id.</value>
		[DataMember]
		public Guid PersonId { get; set; }

		/// <summary>
		/// Gets or sets the mandatory target date which the personal activity activity belongs to.
		/// </summary>
		/// <value>The target date.</value>
		[DataMember]
		public DateOnlyDto Date { get; set; }

		/// <summary>
		/// Gets or sets the mandatory period.
		/// </summary>
		/// <value>The period.</value>
		[DataMember]
		public DateTimePeriodDto Period { get; set; }

		/// <summary>
		/// Gets or sets the scenario id. If omitted default scenario will be used.
		/// </summary>
		[DataMember]
		public Guid? ScenarioId { get; set; }

		/// <summary>
		/// Gets or sets the schedule tag id. If omitted then Null schedule tag will be used.
		/// </summary>
		[DataMember(Order = 2, IsRequired = false)]
		public Guid? ScheduleTagId { get; set; }

		/// <summary>
		/// Gets or sets the Activity id. If omitted then all will be cancelled.
		/// </summary>
		[DataMember(Order = 2, IsRequired = false)]
		public Guid? ActivityId { get; set; }
	}
}