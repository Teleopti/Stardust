using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
	/// <summary>
	/// Query to get a collection of <see cref="SchedulePartDto"/> for a <see cref="GroupPageGroupDto"/>.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2012/04/")]
	public class GetSchedulesByGroupPageGroupQueryDto : QueryDto
	{
		/// <summary>
		/// Gets or sets the mandatory time zone id.
		/// </summary>
		[DataMember]
		public string TimeZoneId { get; set; }

		/// <summary>
		/// Gets or sets the mandatory query date.
		/// </summary>
		[DataMember]
		public DateOnlyDto QueryDate { get; set; }

		/// <summary>
		/// Gets or sets the scenario id. Setting the scenario id to null will use default scenario.
		/// </summary>
		[DataMember]
		public Guid? ScenarioId { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="GroupPageGroupDto"/> id.
		/// </summary>
		/// <remarks>This can also be an Id from a <see cref="TeamDto"/>.</remarks>
		[DataMember]
		public Guid GroupPageGroupId { get; set; }
	}
}
