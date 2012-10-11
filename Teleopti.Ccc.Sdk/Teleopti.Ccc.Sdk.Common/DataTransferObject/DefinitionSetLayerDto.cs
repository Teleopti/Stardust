using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
	/// <summary>
	/// Definition set for overtime (must be included when creating new overtime layers)
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2012/09/")]
	public class DefinitionSetLayerDto : LayerDto
	{
		/// <summary>
		/// Gets or sets the multiplicator Id
		/// </summary>
		/// <value>The Mutliplicator Id.</value>
		[DataMember]
		public Guid MultiplicatorId { get; set; }
	}
}
