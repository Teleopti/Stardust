using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
	/// <summary>
	/// Query to get all application roles.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2014/04/")]
	public class GetAllRolesQueryDto : QueryDto
	{
		/// <summary>
		/// Gets or sets option to include deleted roles in the result.
		/// </summary>
		[DataMember]
		public bool LoadDeleted { get; set; }
	}
}