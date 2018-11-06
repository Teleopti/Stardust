using System;

namespace Teleopti.Wfm.Api.Command
{
	public class RemoveAbsenceDto : ICommandDto
	{
		public Guid PersonId;
		public DateTime UtcStartTime;
		public DateTime UtcEndTime;
		public Guid? ScenarioId;
	}
}