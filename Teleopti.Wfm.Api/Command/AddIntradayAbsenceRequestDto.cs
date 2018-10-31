using System;

namespace Teleopti.Wfm.Api.Command
{
	public class AddIntradayAbsenceRequestDto : ICommandDto
	{
		public Guid PersonId;
		public Guid AbsenceId;
		public DateTime UtcStartTime;
		public DateTime UtcEndTime;
		public string Subject;
		public string Message;
	}
}