using System;
using System.Runtime.Serialization;
using Teleopti.Ccc.Sdk.Common.Contracts;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
	/// <summary>
	/// Represents a projected visual layer for display of schedules
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
	public class ProjectedLayerDto
	{
		/// <summary>
		/// Gets or sets the period.
		/// </summary>
		/// <value>The period.</value>
		[DataMember]
		public DateTimePeriodDto Period { get; set; }

		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		/// <value>The description.</value>
		[DataMember]
		public string Description { get; set; }

		/// <summary>
		/// Gets or sets the display color.
		/// </summary>
		/// <value>The display color.</value>
		[DataMember]
		public ColorDto DisplayColor { get; set; }

		/// <summary>
		/// The layer represents an absence.
		/// </summary>
		/// <value>The absence.</value>
		[DataMember]
		public bool IsAbsence { get; set; }

		/// <summary>
		/// The underlaying ID of the payload for this layer.
		/// </summary>
		/// <value>The payload.</value>
		/// <remarks>The payload details can be found by matching id with lists from <see cref="ITeleoptiSchedulingService.GetActivities"/> and <see cref="ITeleoptiSchedulingService.GetAbsences"/>.</remarks>
		[DataMember]
		public Guid PayloadId { get; set; }

		/// <summary>
		/// Gets or sets the contract time for this layer.
		/// </summary>
		/// <value>The contract time.</value>
		[DataMember]
		public TimeSpan ContractTime { get; set; }

		/// <summary>
		/// Gets or sets the overtime definition set id.
		/// </summary>
		/// <value>The overtime definition set id.</value>
		[DataMember]
		public Guid? OvertimeDefinitionSetId { get; set; }

		/// <summary>
		/// Gets or sets the meeting id.
		/// </summary>
		/// <value>The meeting id.</value>
		/// <remarks>
		/// Created by: henryg
		/// Created date: 2009-10-22
		/// </remarks>
		[DataMember]
		public Guid? MeetingId { get; set; }

		/// <summary>
		/// Gets or sets the work time for this layer.
		/// </summary>
		[DataMember]
		public TimeSpan WorkTime { get; set; }

		/// <summary>
		/// Gets or sets the paid time for this layer.
		/// </summary>
		[DataMember]
		public TimeSpan PaidTime { get; set; }
	}

}