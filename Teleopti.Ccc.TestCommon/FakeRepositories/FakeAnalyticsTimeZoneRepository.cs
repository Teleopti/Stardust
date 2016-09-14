using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsTimeZoneRepository : IAnalyticsTimeZoneRepository
	{
		private readonly List<AnalyticsTimeZone> timeZones = new List<AnalyticsTimeZone>
		{
			new AnalyticsTimeZone {TimeZoneId = 1, TimeZoneCode = "UTC"},
			new AnalyticsTimeZone {TimeZoneId = 2, TimeZoneCode = "W. Europe Standard Time"}
		};

		public AnalyticsTimeZone Get(string timeZoneCode)
		{
			return timeZones.FirstOrDefault(x => x.TimeZoneCode == timeZoneCode);
		}

		public IList<AnalyticsTimeZone> GetAll()
		{
			return timeZones;
		}
	}
}