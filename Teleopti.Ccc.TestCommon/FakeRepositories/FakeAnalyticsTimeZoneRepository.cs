using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsTimeZoneRepository : IAnalyticsTimeZoneRepository
	{
		IList<AnalyticsTimeZone> _dataSourceAndBaseConfigTimeZones = new List<AnalyticsTimeZone>();

		private readonly List<AnalyticsTimeZone> dimTimeZones = new List<AnalyticsTimeZone>
		{
			new AnalyticsTimeZone {TimeZoneId = 1, TimeZoneCode = "UTC"},
			new AnalyticsTimeZone {TimeZoneId = 2, TimeZoneCode = "W. Europe Standard Time"}
		};

		public AnalyticsTimeZone Get(string timeZoneCode)
		{
			var analyticsTimeZone = dimTimeZones.FirstOrDefault(x => x.TimeZoneCode == timeZoneCode);
			if (analyticsTimeZone == null)
			{
				analyticsTimeZone = new AnalyticsTimeZone
				{
					TimeZoneCode = timeZoneCode,
					TimeZoneId = dimTimeZones.Max(t => t.TimeZoneId) + 1 
				};
				dimTimeZones.Add(analyticsTimeZone);
			}
			return analyticsTimeZone;
		}

		public IList<AnalyticsTimeZone> GetAll()
		{
			return dimTimeZones;
		}

		public void SetUtcInUse(bool isUtcInUse)
		{
			var timeZone = dimTimeZones.FirstOrDefault(x => x.TimeZoneCode == "UTC");
			timeZone.IsUtcInUse = isUtcInUse;
		}

		public void SetToBeDeleted(string timeZoneCode, bool tobeDeleted)
		{ 
			var timeZone = dimTimeZones.FirstOrDefault(x => x.TimeZoneCode == timeZoneCode);
			timeZone.ToBeDeleted = tobeDeleted;
		}

		public IList<AnalyticsTimeZone> GetAllUsedByLogDataSourcesAndBaseConfig()
		{
			return _dataSourceAndBaseConfigTimeZones;
		}

		public void SetToBeDeleted(string timeZoneCode)
		{
			throw new System.NotImplementedException();
		}

		public void HasLogDataSourceOrBaseConfigTimeZone(string timeZoneCode)
		{
			_dataSourceAndBaseConfigTimeZones.Add(new AnalyticsTimeZone
			{
				TimeZoneCode = timeZoneCode,
				TimeZoneId = _dataSourceAndBaseConfigTimeZones.Any() ? _dataSourceAndBaseConfigTimeZones.Max(t => t.TimeZoneId) + 1 : 1
			});
		}
	}
}