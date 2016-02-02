using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
	/// <summary>
	/// Specify a query to get a collection of <see cref="PersonDto"/> based on a <see cref="GroupPageGroupDto"/> inside a <see cref="GroupPageDto"/> for a specified date range.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2016/02/")]
	public class GetPeopleByGroupPageGroupForDateRangeQueryDto : QueryDto
	{
		/// <summary>
		/// Gets or sets the mandatory id of the <see cref="GroupPageGroupDto"/> inside the <see cref="GroupPageDto"/>.
		/// </summary>
		[DataMember]
		public Guid GroupPageGroupId{get; set;}

		/// <summary>
		/// Gets or sets the mandatory date range to query for.
		/// </summary>
		[DataMember]
		public DateOnlyPeriodDto QueryRange { get; set; }
	}
}