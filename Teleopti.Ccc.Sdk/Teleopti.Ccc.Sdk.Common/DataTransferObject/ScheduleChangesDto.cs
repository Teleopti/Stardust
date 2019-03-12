using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
	/// <summary>
	/// Represents the ScheduleChangeDto which contains schedules and paging information.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2019/03/")]
	public class ScheduleChangesDto : Dto
	{
		/// <summary>
		/// Contains SchedulePartDto's results.
		/// </summary>
		[DataMember]
		public ICollection<SchedulePartDto> Schedules { get; set; }

		/// <summary>
		/// Which page the resultset <see cref="Schedules"/> contains.
		/// </summary>
		[DataMember]
		public int Page { get; set; }

		/// <summary>
		/// The total number of pages that can be requested with current query parameters.
		/// </summary>
		[DataMember]
		public int TotalPages { get; set; }

		/// <summary>
		/// The total number of schedules that is available with the current query parameters.
		/// </summary>
		[DataMember]
		public int TotalSchedules { get; set; }

		/// <summary>
		/// The point in time up which the result set contains changes for.
		/// </summary>
		[DataMember]
		public DateTime ChangesUpToUTC { get; set; }
	}
}
