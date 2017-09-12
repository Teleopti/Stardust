using System.Collections.Generic;
using Teleopti.Ccc.Domain.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsTimeZoneRepository
	{
		AnalyticsTimeZone Get(string timeZoneCode);
		IList<AnalyticsTimeZone> GetAll();
		void SetUtcInUse();
		void SetToBeDeleted(string timeZoneCode);
	}
}