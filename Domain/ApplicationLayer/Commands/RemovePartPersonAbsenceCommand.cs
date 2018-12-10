using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class RemovePartPersonAbsenceCommand : ITrackableCommand
	{
		public IPerson Person { get; set; }
		public DateTime ScheduleDate { get; set; }
		public IPersonAbsence PersonAbsence { get; set; }
		public DateTimePeriod PeriodToRemove { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
		public ActionErrorMessage Errors { get; set; }
	}
}