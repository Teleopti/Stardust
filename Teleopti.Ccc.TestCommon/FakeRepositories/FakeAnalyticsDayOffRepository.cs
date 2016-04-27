using System.Collections.Generic;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsDayOffRepository : IAnalyticsDayOffRepository
	{
		private readonly List<AnalyticsDayOff> _fakeAnalyticsDayOffs;
		public FakeAnalyticsDayOffRepository()
		{
			_fakeAnalyticsDayOffs = new List<AnalyticsDayOff>();
		}

		public void AddOrUpdate(AnalyticsDayOff analyticsDayOff)
		{
			_fakeAnalyticsDayOffs.RemoveAll(a => a.DayOffCode == analyticsDayOff.DayOffCode);
			_fakeAnalyticsDayOffs.Add(analyticsDayOff);
		}

		public IList<AnalyticsDayOff> DayOffs()
		{
			return _fakeAnalyticsDayOffs;
		}
	}
}