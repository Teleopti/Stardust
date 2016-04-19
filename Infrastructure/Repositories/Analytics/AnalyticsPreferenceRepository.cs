using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsPreferenceRepository : IAnalyticsPreferenceRepository
	{
		private readonly ICurrentDataSource _currentDataSource;

		public AnalyticsPreferenceRepository(ICurrentDataSource currentDataSource)
		{
			_currentDataSource = currentDataSource;
		}

		public void AddPreference(AnalyticsFactSchedulePreference analyticsFactSchedulePreference)
		{
			throw new NotImplementedException();
		}

		public void DeletePreferences(int dateId, int personId)
		{
		}

		public IList<AnalyticsFactSchedulePreference> PreferencesForPerson(int personId)
		{
			throw new NotImplementedException();
		}
	}
}
