using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public class AnalyticsScheduleChangeForDefaultScenarioFilter : IAnalyticsScheduleChangeUpdaterFilter
	{

		public bool ContinueProcessingEvent(bool isDefaultScenario, Guid scenarioId)
		{
			return isDefaultScenario;
		}
	}
}