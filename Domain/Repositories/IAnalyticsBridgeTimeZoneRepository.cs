using System.Collections.Generic;
using Teleopti.Ccc.Domain.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsBridgeTimeZoneRepository
	{
		IList<AnalyticsBridgeTimeZone> GetBridges(int timeZoneId);
		void Save(IList<AnalyticsBridgeTimeZone> toBeAdded);
	}
}