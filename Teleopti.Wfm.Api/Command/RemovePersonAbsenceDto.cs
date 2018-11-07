using System;

namespace Teleopti.Wfm.Api.Command
{
	public class RemovePersonAbsenceDto : ICommandDto
	{
		public Guid PersonId;
		public DateTime UtcStartTime;
		public DateTime UtcEndTime;
		public Guid? ScenarioId;
	}
}