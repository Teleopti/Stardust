using System;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;

namespace Teleopti.Wfm.Api.Command
{
	public class RemoveMeetingDto : ICommandDto
	{
		public DateTime UtcStartTime { get; set; }
		public DateTime UtcEndTime { get; set; }
		public Guid? ScenarioId { get; set; }
		public Guid PersonId { get; set; }
		public Guid MeetingId { get; set; }
	}
}