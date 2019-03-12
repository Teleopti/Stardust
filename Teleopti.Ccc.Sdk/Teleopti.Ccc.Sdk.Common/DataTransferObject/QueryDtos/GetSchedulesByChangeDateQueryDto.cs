using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
	/// <summary>
	/// Query to get a collection of schedule changes, <see cref="SchedulePartDto"/>, from a certain point in time.
	/// <see cref="GetSchedulesByChangeDateQueryDto"/> supports two querying modes. Paged and non-paged. 
	/// See <see cref="Page"/> on how mode is selected.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2019/03/")]
	public class GetSchedulesByChangeDateQueryDto : QueryDto
	{
		/// <summary>
		/// Gets or sets the point in time from which changes should be retreived.
		/// </summary>
		[DataMember]
		public DateTime ChangesFromUTC { get; set; }

		/// <summary>
		/// Gets or sets the point in time to which changes should be retreived. 
		/// If omitted or set before <see cref="ChangesFromUTC"/> it will be ignored and all available changes up to utc now will be retreived.
		/// If set in the far future it will be set back to the current utc now time.
		/// </summary>
		[DataMember]
		public DateTime ChangesToUTC { get; set; }

		/// <summary>
		/// Gets or sets the pagenumber to get data for. The page property is 1-based. 
		/// If set to '0', no paging will occur and all matching data will be received in a single go.
		/// If set beyond the total number of available pages an empty result set will be returned.
		/// </summary>
		[DataMember]
		public int Page { get; set; }

		/// <summary>
		/// Gets or sets the size of the requested page. 
		/// If a valid page is set, the pageSize will determine the approximated number of days on each page. Each returned page will be slightly larger.
		/// PageSize is ignored when <see cref="Page"/> is set to '0'.
		/// </summary>
		[DataMember]
		public int PageSize { get; set; }
	}
}