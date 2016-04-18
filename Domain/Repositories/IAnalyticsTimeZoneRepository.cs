using Teleopti.Ccc.Domain.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsTimeZoneRepository
	{
		AnalyticsTimeZone Get(string id);
	}

}