using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public interface IAnalyticsFactScheduleTimeHandler
	{
		AnalyticsFactScheduleTime Handle(ProjectionChangedEventLayer layer);
	}

	public class AnalyticsFactScheduleTimeHandler : IAnalyticsFactScheduleTimeHandler
	{
		public AnalyticsFactScheduleTime Handle(ProjectionChangedEventLayer layer)
		{
			return new AnalyticsFactScheduleTime();
		}
	}
}