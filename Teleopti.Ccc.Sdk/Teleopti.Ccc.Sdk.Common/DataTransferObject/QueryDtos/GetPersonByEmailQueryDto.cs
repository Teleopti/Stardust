using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
	/// <summary>
	/// Specify a query to get a <see cref="PersonDto"/> by email.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
	public class GetPersonByEmailQueryDto
	{
		/// <summary>
		/// Gets and sets the mandatory person email.
		/// </summary>
		/// <value>The person's email.</value>
		[DataMember]
		public string Email { get; set; }
	}
}