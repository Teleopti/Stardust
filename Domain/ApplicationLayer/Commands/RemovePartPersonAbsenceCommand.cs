using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class RemovePartPersonAbsenceCommand : ITrackableCommand
	{
		public Guid PersonAbsenceId { get; set; }
		public DateTimePeriod PeriodToRemove { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
	}
}