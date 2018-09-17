using System;

namespace Teleopti.Wfm.Api.Command
{
	public class AddAbsenceDto : ICommandDto
	{
		public Guid PersonId;
		public DateTime UtcStartTime;
		public DateTime UtcEndTime;
		public Guid AbsenceId;
		public Guid? ScenarioId;
	}
}