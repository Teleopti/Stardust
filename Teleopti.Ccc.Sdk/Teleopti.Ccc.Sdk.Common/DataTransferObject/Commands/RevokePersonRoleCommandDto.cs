using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
	/// <summary>
	/// This command revokes a role to an existing person. 
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2014/04/")]
	public class RevokePersonRoleCommandDto : CommandDto
	{
		/// <summary>
		/// Gets or sets the mandatory person id.
		/// </summary>
		[DataMember(IsRequired = true)]
		public Guid PersonId { get; set; }

		/// <summary>
		/// Gets or sets the mandatory role id.
		/// </summary>
		[DataMember(IsRequired = true)]
		public Guid RoleId { get; set; }
	}
}