using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public class AnalyticsScheduleChangeForDefaultScenarioFilter : IAnalyticsScheduleChangeUpdaterFilter
	{
		public bool ContinueProcessingEvent(ProjectionChangedEvent @event)
		{
			return @event.IsDefaultScenario;
		}
	}
}