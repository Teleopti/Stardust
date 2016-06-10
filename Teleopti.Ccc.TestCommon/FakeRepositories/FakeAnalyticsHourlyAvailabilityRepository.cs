using System.Collections.Generic;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsHourlyAvailabilityRepository : IAnalyticsHourlyAvailabilityRepository
	{
		public readonly List<AnalyticsHourlyAvailability> AnalyticsHourlyAvailabilities;

		public FakeAnalyticsHourlyAvailabilityRepository()
		{
			AnalyticsHourlyAvailabilities = new List<AnalyticsHourlyAvailability>();
		}

		public void Delete(int personId, int dateId, int scenarioId)
		{
			AnalyticsHourlyAvailabilities.RemoveAll(x => x.PersonId == personId && x.DateId == dateId && x.ScenarioId == scenarioId);
		}

		public void AddOrUpdate(AnalyticsHourlyAvailability analyticsHourlyAvailability)
		{
			Delete(analyticsHourlyAvailability.PersonId, analyticsHourlyAvailability.DateId, analyticsHourlyAvailability.ScenarioId);
			AnalyticsHourlyAvailabilities.Add(analyticsHourlyAvailability);
		}
	}
}