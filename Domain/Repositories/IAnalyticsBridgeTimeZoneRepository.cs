using System.Collections.Generic;
using Teleopti.Ccc.Domain.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsBridgeTimeZoneRepository
	{
		IList<AnalyticsBridgeTimeZonePartial> GetBridgesPartial(int timeZoneId, int minDateId);
		void Save(IList<AnalyticsBridgeTimeZone> toBeAdded);
		int GetMaxDateForTimeZone(int timeZoneId);
	}
}