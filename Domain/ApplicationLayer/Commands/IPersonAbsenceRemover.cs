using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public interface IPersonAbsenceRemover
	{
		IEnumerable<string> RemovePersonAbsence(DateTime scheduleDate, IPerson person,
			IEnumerable<IPersonAbsence> personAbsences, TrackedCommandInfo commandInfo = null);

		IEnumerable<string> RemovePartPersonAbsence(DateTime scheduleDate, IPerson person,
			IEnumerable<IPersonAbsence> personAbsences, DateTimePeriod periodToRemove,
			TrackedCommandInfo commandInfo = null);
	}
}