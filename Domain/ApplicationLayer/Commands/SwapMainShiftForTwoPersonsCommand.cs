using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class SwapMainShiftForTwoPersonsCommand : ITrackableCommand
	{
		public TrackedCommandInfo TrackedCommandInfo { get; set; }

		public Guid PersonIdFrom { get; set; }
		public Guid PersonIdTo { get; set; }
		public DateTime ScheduleDate { get; set; }
	}
}