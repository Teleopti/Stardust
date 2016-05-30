using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsTimeZoneRepository : IAnalyticsTimeZoneRepository
	{
		public AnalyticsTimeZone Get(string id)
		{
			return new AnalyticsTimeZone {TimeZoneId = 1};
		}
	}
}