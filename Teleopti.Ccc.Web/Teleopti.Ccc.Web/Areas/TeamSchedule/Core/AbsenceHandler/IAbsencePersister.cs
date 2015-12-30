using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core.AbsenceHandler
{
	public interface IAbsencePersister
	{
		FailActionResult PersistFullDayAbsence(AddFullDayAbsenceCommand command);
		FailActionResult PersistIntradayAbsence(AddIntradayAbsenceCommand command);
	}
}