using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
	/// <summary>
	/// Query to get all application roles for a person.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2014/04/")]
	public class GetPersonRolesQueryDto : QueryDto
	{
		/// <summary>
		/// Gets or sets the person id.
		/// </summary>
		[DataMember]
		public Guid PersonId { get; set; }
	}
}