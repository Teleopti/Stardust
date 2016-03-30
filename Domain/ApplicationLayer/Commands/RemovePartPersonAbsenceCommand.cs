using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class RemovePartPersonAbsenceCommand : ITrackableCommand
	{
		public IEnumerable<Guid> PersonAbsenceIds { get; set; }
		public DateTimePeriod PeriodToRemove { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
	}
}