using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public interface IAnalyticsFactScheduleDayCountHandler
	{
		IAnalyticsFactScheduleDayCount Handle(ProjectionChangedEventScheduleDay scheduleDay, IAnalyticsFactSchedulePerson personPart, int scenarioId, int shiftCategoryId);
	}
}