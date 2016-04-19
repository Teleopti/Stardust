using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsPreferenceRepository
	{
		void AddPreference(AnalyticsFactSchedulePreference analyticsFactSchedulePreference);
		void DeletePreferences(int dateId, int personId);
		IList<AnalyticsFactSchedulePreference> PreferencesForPerson(int personId);
	}
}