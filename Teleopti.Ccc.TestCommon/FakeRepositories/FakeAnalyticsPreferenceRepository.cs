using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsPreferenceRepository : IAnalyticsPreferenceRepository
	{
		private readonly List<AnalyticsFactSchedulePreference> fakeAnalyticsFactSchedulePreferences; 
		public FakeAnalyticsPreferenceRepository()
		{
			fakeAnalyticsFactSchedulePreferences = new List<AnalyticsFactSchedulePreference>();
		}

		public void AddPreference(AnalyticsFactSchedulePreference analyticsFactSchedulePreference)
		{
			fakeAnalyticsFactSchedulePreferences.Add(analyticsFactSchedulePreference);
		}

		public void DeletePreferences(int dateId, int personId, int? scenarioId = null)
		{
			if (scenarioId == null)
				fakeAnalyticsFactSchedulePreferences.RemoveAll(a => a.DateId == dateId && a.PersonId == personId);
			else
				fakeAnalyticsFactSchedulePreferences.RemoveAll(a => a.DateId == dateId && a.PersonId == personId && a.ScenarioId == scenarioId);
		}

		public IList<AnalyticsFactSchedulePreference> PreferencesForPerson(int personId)
		{
			return fakeAnalyticsFactSchedulePreferences.Where(a => a.PersonId == personId).ToList();
		}
	}
}