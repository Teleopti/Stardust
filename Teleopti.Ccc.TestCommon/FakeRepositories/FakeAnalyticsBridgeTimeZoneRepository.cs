using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsBridgeTimeZoneRepository : IAnalyticsBridgeTimeZoneRepository
	{
		public readonly HashSet<AnalyticsBridgeTimeZone> Bridges = new HashSet<AnalyticsBridgeTimeZone>();

		public IList<AnalyticsBridgeTimeZonePartial> GetBridgesPartial(int timeZoneId)
		{
			return Bridges.Where(x => x.TimeZoneId == timeZoneId).Cast<AnalyticsBridgeTimeZonePartial>().ToList();
		}

		public void Save(IList<AnalyticsBridgeTimeZone> toBeAdded)
		{
			foreach (var item in toBeAdded)
			{
				if (!Bridges.Contains(item))
					Bridges.Add(item);
			}
		}
	}
}