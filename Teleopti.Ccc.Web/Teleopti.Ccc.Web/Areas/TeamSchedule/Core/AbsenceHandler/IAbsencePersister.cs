using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core.AbsenceHandler
{
	public interface IAbsencePersister
	{
		AddAbsenceFailResult PersistFullDayAbsence(AddFullDayAbsenceCommand command);
		AddAbsenceFailResult PersistIntradayAbsence(AddIntradayAbsenceCommand command);
	}
}