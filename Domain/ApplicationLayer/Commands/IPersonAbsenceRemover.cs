using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public interface IPersonAbsenceRemover
	{
		void RemovePersonAbsence(IPersonAbsence personAbsence, TrackedCommandInfo commandInfo = null);
		void RemovePartPersonAbsence(IPersonAbsence personAbsence, DateTimePeriod periodToRemove, TrackedCommandInfo commandInfo = null);
	}
}