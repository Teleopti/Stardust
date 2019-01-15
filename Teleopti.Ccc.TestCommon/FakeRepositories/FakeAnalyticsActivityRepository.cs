using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsActivityRepository : IAnalyticsActivityRepository
	{
		private readonly List<AnalyticsActivity> fakeActivities;

		public FakeAnalyticsActivityRepository()
		{
			fakeActivities = new List<AnalyticsActivity>();
		}

		public AnalyticsActivity Activity(Guid code)
		{
			return fakeActivities.Find(x => x.ActivityCode == code);
		}

		public IList<AnalyticsActivity> Activities()
		{
			return fakeActivities;
		}

		public void AddActivity(AnalyticsActivity activity)
		{
			activity.ActivityId = fakeActivities.Any() ? fakeActivities.Max(a => a.ActivityId) + 1 : 1;
			fakeActivities.Add(activity);
		}

		public void UpdateActivity(AnalyticsActivity activity)
		{
			activity.ActivityId = fakeActivities.First(a => a.ActivityCode == activity.ActivityCode).ActivityId;
			fakeActivities.RemoveAll(a => a.ActivityCode == activity.ActivityCode);
			fakeActivities.Add(activity);
		}
	}
}