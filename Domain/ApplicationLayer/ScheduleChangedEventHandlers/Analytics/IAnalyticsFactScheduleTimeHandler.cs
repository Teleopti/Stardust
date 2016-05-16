using System;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public interface IAnalyticsFactScheduleTimeHandler
	{
		IAnalyticsFactScheduleTime Handle(ProjectionChangedEventLayer layer, int shiftCategoryId, int scenarioId, int shiftLength);
		AnalyticsAbsence MapAbsenceId(Guid absenceCode);
		int MapOvertimeId(Guid overtimeCode);
	}
}