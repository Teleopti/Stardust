using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/10/")]
	public class RecalculateForecastOnSkillCommandDto : CommandDto
	{
		[DataMember]
		public Guid SkillId { get; set; }

		[DataMember]
		public DateTimePeriodDto Period { get; set; }

		[DataMember]
		public Guid OwnerPersonId { get; set; }

		[DataMember]
		public Guid ScenarioId { get; set; }

		// add information how the forecast should change
		// percent up/down or some other way
	}
}