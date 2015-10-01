using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
	/// <summary>
	/// This command end person employment.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/10/")]
	public class EndPersonEmploymentCommandDto : CommandDto
	{
		/// <summary>
		/// Gets or sets the mandatory person id.
		/// </summary>
		/// <value>The person id.</value>
		[DataMember]
		public Guid PersonId { get; set; }
		/// <summary>
		/// Gets or sets the mandatory terminate date
		/// </summary>
		/// <value>The terminate date.</value>
		[DataMember]
		public DateOnlyDto Date { get; set; }
	}
}