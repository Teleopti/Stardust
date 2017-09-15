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
			var analyticsTimeZone = timeZones.FirstOrDefault(x => x.TimeZoneCode == timeZoneCode);
			if (analyticsTimeZone == null)
			{
				analyticsTimeZone = new AnalyticsTimeZone
				{
					TimeZoneCode = timeZoneCode,
					TimeZoneId = timeZones.Max(t => t.TimeZoneId) + 1 
				};
				timeZones.Add(analyticsTimeZone);
			}
			return analyticsTimeZone;
		}

		public IList<AnalyticsTimeZone> GetAll()
		{
			return timeZones;
		}

		public void SetUtcInUse(bool isUtcInUse)
		{
			var timeZone = timeZones.FirstOrDefault(x => x.TimeZoneCode == "UTC");
			timeZone.IsUtcInUse = isUtcInUse;
		}

		public void SetToBeDeleted(string timeZoneCode, bool tobeDeleted)
		{ 
			var timeZone = timeZones.FirstOrDefault(x => x.TimeZoneCode == timeZoneCode);
			timeZone.ToBeDeleted = tobeDeleted;
		}

		public void SetToBeDeleted(string timeZoneCode)
		{
			throw new System.NotImplementedException();
		}
	}
}