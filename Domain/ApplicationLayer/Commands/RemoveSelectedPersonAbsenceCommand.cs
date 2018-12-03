using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class RemoveSelectedPersonAbsenceCommand : ITrackableCommand, IErrorAttachedCommand
	{
		public DateOnly Date { get; set; }
		public Guid PersonAbsenceId { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
		public IList<string> ErrorMessages { get; set; }
		public IPerson Person { get; set; }
		public IScheduleRange ScheduleRange { get; set; }
	}
}
