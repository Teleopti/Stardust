using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
	/// <summary>
	/// Definition set for overtime (must be included when creating new overtime layers)
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2012/09/")]
	public class DefinitionSetLayerDto : Dto
	{
		/// <summary>
		/// Gets or sets the date time period
		/// </summary>
		/// <value>The DateTimePeriodDto.</value>
		[DataMember]
		public DateTimePeriodDto DateTimePeriod { get; set; }

		/// <summary>
		/// Gets or sets the multiplicator Id
		/// </summary>
		/// <value>The Mutliplicator Id.</value>
		[DataMember]
		public Guid MultiplicatorId { get; set; }
	}
}
