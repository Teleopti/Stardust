using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2014/04/")]
	public class RoleDto : Dto
	{
		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		[DataMember]
		public string Name { get; set; }

	}
}