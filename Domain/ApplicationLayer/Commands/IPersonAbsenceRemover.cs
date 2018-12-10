using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public interface IPersonAbsenceRemover
	{
		IEnumerable<string> RemovePersonAbsence (DateOnly scheduleDate, IPerson person,
			IPersonAbsence personAbsence, IScheduleRange scheduleRange, TrackedCommandInfo commandInfo = null);

		IEnumerable<string> RemovePartPersonAbsence (DateOnly scheduleDate, IPerson person,
			IPersonAbsence personAbsence, DateTimePeriod periodToRemove, IScheduleRange scheduleRange,
			TrackedCommandInfo commandInfo = null);
	}
}