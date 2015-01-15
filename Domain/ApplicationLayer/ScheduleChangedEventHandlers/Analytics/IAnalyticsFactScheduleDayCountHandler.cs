using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public interface IAnalyticsFactScheduleDayCountHandler
	{
		IAnalyticsFactScheduleDayCount Handle(ProjectionChangedEventScheduleDay scheduleDay, IAnalyticsFactSchedulePerson personPart, int scenarioId, int shiftCategoryId);
	}
}