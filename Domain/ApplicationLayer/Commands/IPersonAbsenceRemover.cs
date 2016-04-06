using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public interface IPersonAbsenceRemover
	{
		IEnumerable<string> RemovePersonAbsence(IPersonAbsence personAbsence, TrackedCommandInfo commandInfo = null);
		IEnumerable<string> RemovePartPersonAbsence(IPersonAbsence personAbsence, DateTimePeriod periodToRemove, TrackedCommandInfo commandInfo = null);
	}
}