using System.Collections.Generic;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsOvertimeRepository : IAnalyticsOvertimeRepository
	{
		private readonly List<AnalyticsOvertime> overtimes = new List<AnalyticsOvertime>();

		public void AddOrUpdate(AnalyticsOvertime analyticsOvertime)
		{
			overtimes.RemoveAll(
				x => x.OvertimeId == analyticsOvertime.OvertimeId && analyticsOvertime.OvertimeCode == x.OvertimeCode);
			overtimes.Add(analyticsOvertime);
		}

		public IList<AnalyticsOvertime> Overtimes()
		{
			return overtimes;
		}
	}
}