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

		/// <summary>
		/// Gets or sets the option to clear all data related to the agent after the leaving date
		/// </summary>
		/// <value>The indication if the clearing of future data should be performed.</value>
		[DataMember(Order = 1,IsRequired = false)]
		public bool ClearAfterLeavingDate { get; set; }
	}
}