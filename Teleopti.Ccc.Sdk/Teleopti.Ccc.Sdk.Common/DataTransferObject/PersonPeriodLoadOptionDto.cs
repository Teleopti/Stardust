using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
	/// <summary>
	/// Load options for person period
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/10/")]
	public class PersonPeriodLoadOptionDto
	{
		/// <summary>
		/// Gets or sets if periods for all agents should be loaded
		/// </summary>
		/// <remarks>Currently no person periods are loaded if this is set to false.</remarks>
		[DataMember]
		public bool LoadAll { get; set; }
	}
}
