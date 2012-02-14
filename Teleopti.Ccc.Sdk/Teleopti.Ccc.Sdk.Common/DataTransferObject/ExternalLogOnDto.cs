using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// The details for an external log on.
    /// </summary>
    /// <remarks>Connects a person to users in peripheral systems.</remarks>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
	public class ExternalLogOnDto : Dto
	{
		/// <summary>
		/// Gets or sets Acd log on original id
		/// </summary>
		[DataMember]
		public string AcdLogOnOriginalId { get; set; }

		/// <summary>
		/// Gets or sets Acd log on name
		/// </summary>
		[DataMember]
		public string AcdLogOnName { get; set; }
	}
}
