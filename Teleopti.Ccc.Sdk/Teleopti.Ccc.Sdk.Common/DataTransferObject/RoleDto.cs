using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
	/// <summary>
	/// Represents an application role.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2014/04/")]
	public class RoleDto : Dto
	{
		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		[DataMember]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the flag to determine if this role is deleted.
		/// </summary>
		/// <value>The is deleted flag.</value>
		[DataMember]
		public bool IsDeleted { get; set; }
	}
}