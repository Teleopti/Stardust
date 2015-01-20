using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public interface IAnalyticsFactScheduleHandler
	{
		List<IFactScheduleRow> AgentDaySchedule(
			ProjectionChangedEventScheduleDay scheduleDay,
			IAnalyticsFactSchedulePerson personPart,
			DateTime scheduleChangeTime,
			int shiftCategoryId,
			int scenarioId);
	}
}