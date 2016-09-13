using System.Collections.Generic;
using Teleopti.Ccc.Domain.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsBridgeTimeZoneRepository
	{
		IList<AnalyticsBridgeTimeZonePartial> GetBridgesPartial(int timeZoneId);
		void Save(IList<AnalyticsBridgeTimeZone> toBeAdded);
	}
}