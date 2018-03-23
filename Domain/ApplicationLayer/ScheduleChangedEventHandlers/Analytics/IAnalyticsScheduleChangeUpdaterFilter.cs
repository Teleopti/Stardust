using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public interface IAnalyticsScheduleChangeUpdaterFilter
	{
		bool ContinueProcessingEvent(bool isDefaultScenario, Guid scenarioId);
	}
}