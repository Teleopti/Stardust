using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public class AnalyticsFactSchedulePerson : IAnalyticsFactSchedulePerson
	{
		public AnalyticsFactSchedulePerson()
		{
			PersonId = -1;
			BusinessUnitId = -1;
		}
		public int PersonId { get; set; }
		public int BusinessUnitId { get; set; }
	}
}