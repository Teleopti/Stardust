using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public interface IPersonAbsenceRemover
	{
		IEnumerable<string> RemovePersonAbsence (DateOnly scheduleDate, IPerson person,
			IEnumerable<IPersonAbsence> personAbsences, IScheduleRange scheduleRange, TrackedCommandInfo commandInfo = null);

		IEnumerable<string> RemovePartPersonAbsence (DateOnly scheduleDate, IPerson person,
			IEnumerable<IPersonAbsence> personAbsences, DateTimePeriod periodToRemove, IScheduleRange scheduleRange,
			TrackedCommandInfo commandInfo = null);
	}
}