﻿using System;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public interface IAnalyticsFactScheduleTimeHandler
	{
		IAnalyticsFactScheduleTime Handle(ProjectionChangedEventLayer layer, int shiftCategoryId, int scenarioId, int shiftLength);
		IAnalyticsAbsence MapAbsenceId(Guid absenceCode);
		int MapOvertimeId(Guid overtimeCode);
	}
}