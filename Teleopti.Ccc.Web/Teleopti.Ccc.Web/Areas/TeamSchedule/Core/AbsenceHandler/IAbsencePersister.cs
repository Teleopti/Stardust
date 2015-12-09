using Teleopti.Ccc.Domain.ApplicationLayer.Commands;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core.AbsenceHandler
{
	public interface IAbsencePersister
	{
		void PersistFullDayAbsence(AddFullDayAbsenceCommand command);
		void PersistIntradayAbsence(AddIntradayAbsenceCommand command);
	}
}