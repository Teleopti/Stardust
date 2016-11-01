using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
	/// <summary>
	/// This command sets external logon for person periods
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2016/11/")]
	public class SetPersonExternalLogOnCommandDto : CommandDto
	{
		/// <summary>
		/// Gets or sets the mandatory person Id.
		/// </summary>
		/// <value>The person Id.</value>
		[DataMember]
		public Guid PersonId { get; set; }

		/// <summary>
		/// Gets or sets external logon list. The <see cref="ExternalLogOnDto"/> must match an existing external log on available.
		/// </summary>
		[DataMember]
		public IList<ExternalLogOnDto> ExternalLogOn { get; set; }

		/// <summary>
		/// Gets or sets the date of the person period to set <see cref="ExternalLogOnDto"/> for.
		/// </summary>
		[DataMember]
		public DateOnlyDto PeriodStartDate { get; set; }
	}
}