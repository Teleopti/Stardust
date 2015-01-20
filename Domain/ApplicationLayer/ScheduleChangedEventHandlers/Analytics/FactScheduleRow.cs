using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public class FactScheduleRow : IFactScheduleRow
	{
		public IAnalyticsFactScheduleDate DatePart { get; set; }
		public IAnalyticsFactScheduleTime TimePart { get; set; }
		public IAnalyticsFactSchedulePerson PersonPart { get; set; }
	}
}