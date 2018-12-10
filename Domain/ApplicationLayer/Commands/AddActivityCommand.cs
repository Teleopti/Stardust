using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class AddActivityCommand : ITrackableCommand, IErrorAttachedCommand
	{
		public IPerson Person { get; set; }
		public DateOnly Date { get; set; }
		public Guid ActivityId { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
		public bool MoveConflictLayerAllowed { get; set; }
		public IList<string> ErrorMessages { get; set; }
		public IList<string> WarningMessages { get; set; }
	}

	public class AddActivityCommandSimply :  IScheduleCommand
	{
		public IActivity Activity { get; set; }
		public DateTimePeriod Period { get; set; }
	}
}