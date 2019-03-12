using System;

namespace Teleopti.Wfm.Api.Command
{
	public class AddMeetingDto : ICommandDto
	{
		public Guid MeetingId { get; set; }
		public Guid? ScenarioId { get; set; }
		public Guid PersonId { get; set; }
		public Guid ActivityId { get; set; }
		public DateTime UtcStartTime { get; set; }
		public DateTime UtcEndTime { get; set; }
	}
}