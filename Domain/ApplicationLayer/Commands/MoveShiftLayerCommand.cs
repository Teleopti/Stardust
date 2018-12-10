using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class MoveShiftLayerCommand : ITrackableCommand, IErrorAttachedCommand
	{
		public Guid AgentId { get; set; }
		public DateOnly ScheduleDate { get; set; }
		public Guid ShiftLayerId { get; set; }
		public DateTime NewStartTimeInUtc { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
		public IList<string> ErrorMessages { get; set; }
	}
}