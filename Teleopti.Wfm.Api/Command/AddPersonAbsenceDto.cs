using System;

namespace Teleopti.Wfm.Api.Command
{
	public class AddPersonAbsenceDto : ICommandDto
	{
		public Guid PersonId;
		public DateTime UtcStartTime;
		public DateTime UtcEndTime;
		public Guid AbsenceId;
		public Guid? ScenarioId;
	}
}