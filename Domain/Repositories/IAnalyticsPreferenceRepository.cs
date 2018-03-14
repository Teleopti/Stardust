using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsPreferenceRepository
	{
		void AddPreference(AnalyticsFactSchedulePreference analyticsFactSchedulePreference);
		void DeletePreferences(int dateId, Guid personId, int? scenarioId = null);
		IList<AnalyticsFactSchedulePreference> PreferencesForPerson(int personId);
		void UpdateUnlinkedPersonids(int[] personPeriodIds);
	}
}