using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
	/// <summary>
	/// Query to get schedules for a group page group.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2012/04/")]
	public class GetSchedulesByGroupPageGroupQueryHandlerDto : QueryDto
	{
		/// <summary>
		/// Gets or sets the time zone id.
		/// </summary>
		[DataMember]
		public string TimeZoneId { get; set; }

		/// <summary>
		/// Gets or sets the query date.
		/// </summary>
		[DataMember]
		public DateOnlyDto QueryDate { get; set; }

		/// <summary>
		/// Gets or sets the scenario id. Setting the scenario id to null will use default scenario.
		/// </summary>
		[DataMember]
		public Guid? ScenarioId { get; set; }

		/// <summary>
		/// Gets or sets the group page group id.
		/// </summary>
		[DataMember]
		public Guid GroupPageGroupId { get; set; }
	}
}
