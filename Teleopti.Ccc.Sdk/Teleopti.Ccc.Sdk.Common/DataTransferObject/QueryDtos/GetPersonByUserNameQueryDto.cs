using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
	/// <summary>
	/// Specify a query to get a <see cref="PersonDto"/> by user name.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
	public class GetPersonByUserNameQueryDto : QueryDto
	{
		/// <summary>
		/// Gets and sets the mandatory person user name.
		/// </summary>
		/// <value>The person's user name.</value>
		[DataMember]
		public string UserName
		{
			get;
			set;
		}
	}
}