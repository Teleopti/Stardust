using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
	public class DenormalizeScheduleCommandDto : CommandDto
	{
		[DataMember]
		public Guid ScenarioId { get; set; }

		[DataMember]
		public Guid PersonId { get; set; }

		[DataMember]
		public DateTime StartDateTime { get; set; }

		[DataMember]
		public DateTime EndDateTime { get; set; }
	}
}