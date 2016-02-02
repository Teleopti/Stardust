using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
	/// <summary>
	/// Query for the <see cref="GroupPageGroupDto"/> inside a group page for a specified date range.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2016/02/")]
	public class GetGroupsForGroupPageForDateRangeQueryDto : QueryDto
	{
		/// <summary>
		/// Gets or sets the mandatory date range to query for.
		/// </summary>
		[DataMember]
		public DateOnlyPeriodDto QueryRange { get; set; }

		/// <summary>
		/// Gets or sets the mandatory group page id to list the groups for.
		/// </summary>
		[DataMember]
		public Guid PageId { get; set; }
	}
}