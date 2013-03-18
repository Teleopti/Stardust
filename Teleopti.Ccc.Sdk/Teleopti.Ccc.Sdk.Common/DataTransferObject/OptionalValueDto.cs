using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
	/// <summary>
	/// Contains a key/value pair of optional values.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2013/03/")]
	public class OptionalValueDto
	{
		/// <summary>
		/// Gets and sets the key for the optional column.
		/// </summary>
		[DataMember]
		public string Key { get; set; }

		/// <summary>
		/// Gets and sets the value for the optional column.
		/// </summary>
		[DataMember]
		public string Value { get; set; }
	}
}