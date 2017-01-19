using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
	/// <summary>
	/// Specify a query to get a list of <see cref="PersonDto"/> by their Ids.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
	public class GetPersonsByIdsQueryDto : QueryDto
	{
		/// <summary>
		/// Gets and sets the mandatory person Ids.
		/// </summary>
		/// <value>The people's Ids.</value>
		[DataMember]
		public ICollection<Guid> PersonIds
		{
			get; set;
		}
	}
}