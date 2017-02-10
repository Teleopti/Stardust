using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
	/// <summary>
	/// Specify a query to get a <see cref="PersonDto"/> by identity.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
	public class GetPersonsByIdentitiesQueryDto : QueryDto
	{
		/// <summary>
		/// Gets and sets the mandatory person identity.
		/// </summary>
		/// <value>The person's identity.</value>
		[DataMember]
		public ICollection<string> Identities { get; set; }
	}
}