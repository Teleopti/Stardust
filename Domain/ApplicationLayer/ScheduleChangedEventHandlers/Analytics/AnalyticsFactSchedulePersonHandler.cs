using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public interface IAnalyticsFactSchedulePersonHandler
	{
		AnalyticsFactSchedulePerson Handle(ProjectionChangedEventLayer layer);
	}

	public class AnalyticsFactSchedulePersonHandler : IAnalyticsFactSchedulePersonHandler
	{
		public AnalyticsFactSchedulePerson Handle(ProjectionChangedEventLayer layer)
		{
			return new AnalyticsFactSchedulePerson();
		}
	}
}