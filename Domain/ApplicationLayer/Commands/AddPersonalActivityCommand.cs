using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class AddPersonalActivityCommand : IErrorAttachedCommand, ITrackableCommand
	{
		public IPerson Person { get; set; }
		public DateOnly Date { get; set; }
		public Guid PersonalActivityId { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
		public IList<string> ErrorMessages { get; set; }
	}

	public class AddPersonalActivityCommandSimply : IScheduleCommand {
		public IActivity Activity { get; set; }
		public DateTimePeriod Period { get; set; }
	}
}