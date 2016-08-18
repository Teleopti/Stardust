using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Collection;
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
			var current = _fakeAnalyticsDayOffs.FirstOrDefault(a => a.DayOffCode == analyticsDayOff.DayOffCode);

			int id;
			if (current != null)
			{
				id = current.DayOffId;
			}
			else
			{
				id = _fakeAnalyticsDayOffs.IsEmpty() ? 1 : _fakeAnalyticsDayOffs.Max(a => a.DayOffId) + 1;
			}

			_fakeAnalyticsDayOffs.RemoveAll(a => a.DayOffCode == analyticsDayOff.DayOffCode);
			analyticsDayOff.DayOffId = id;
			_fakeAnalyticsDayOffs.Add(analyticsDayOff);
		}

		public void AddNotDefined()
		{
			if (_fakeAnalyticsDayOffs.Any(a => a.DayOffId == -1))
				throw new Exception("Could not insert with same id. Sql unique.");
			_fakeAnalyticsDayOffs.Add(new AnalyticsDayOff
			{
				DayOffId = -1,
				DatasourceId = -1,
				DayOffCode = Guid.Empty,
				DatasourceUpdateDate = DateTime.Today,
				BusinessUnitId = -1,
				DayOffName = "Not Defined",
				DayOffShortname = "Not Defined",
				DisplayColor = -1,
				DisplayColorHtml = "#FFFFFF"
			});
		}

		public IList<AnalyticsDayOff> DayOffs()
		{
			return _fakeAnalyticsDayOffs;
		}
	}
}