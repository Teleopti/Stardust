using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class MoveShiftLayerCommand : ITrackableCommand
	{
		public Guid AgentId { get; set; }
		public DateOnly ScheduleDate { get; set; }
		public Guid ShiftLayerId { get; set; }
		public DateTime NewStartTime { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
	}
}