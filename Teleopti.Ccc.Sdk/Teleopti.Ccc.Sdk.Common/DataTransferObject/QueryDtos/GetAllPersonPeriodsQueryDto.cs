using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
	/// <summary>
	/// Gets all the person periods within the given date range.
	/// </summary>
	/// <remarks>This is here for legacy reasons. Please use the <see cref="GetPersonPeriodsByPersonIdQueryDto"/> instead.</remarks>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2016/02/"), Obsolete("Use GetPersonPeriodsByPersonIdQueryDto instead.")]
	public class GetAllPersonPeriodsQueryDto : QueryDto
	{
		/// <summary>
		/// The date range to look for person periods in
		/// </summary>
		[DataMember]
		public DateOnlyPeriodDto Range { get; set; }
	}
}