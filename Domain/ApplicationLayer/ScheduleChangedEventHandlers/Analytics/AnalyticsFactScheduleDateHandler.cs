using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public interface IAnalyticsFactScheduleDateHandler
	{
		AnalyticsFactScheduleDate Handle(ProjectionChangedEventLayer layer);
	}

	public class AnalyticsFactScheduleDateHandler : IAnalyticsFactScheduleDateHandler
	{
		public AnalyticsFactScheduleDate Handle(ProjectionChangedEventLayer layer)
		{
			return new AnalyticsFactScheduleDate();
		}
	}
}