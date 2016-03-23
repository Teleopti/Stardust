using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public interface IPersonAbsenceRemover
	{
		void RemovePersonAbsence(IPersonAbsence personAbsence, TrackedCommandInfo commandInfo = null);
	}
}