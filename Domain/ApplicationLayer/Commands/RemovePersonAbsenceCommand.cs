using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class RemovePersonAbsenceCommand : ITrackableCommand
	{
		public DateTime ScheduleDate { get; set; }
		public IPerson Person { get; set; }
		public IPersonAbsence PersonAbsence { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
		public ActionErrorMessage Errors { get; set; }
	}
}