using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsHourlyAvailabilityRepository : IAnalyticsHourlyAvailabilityRepository
	{
		public readonly List<AnalyticsHourlyAvailability> AnalyticsHourlyAvailabilities;
		public readonly Dictionary<int, Guid> personIdAndCode = new Dictionary<int, Guid>();

		public FakeAnalyticsHourlyAvailabilityRepository()
		{
			AnalyticsHourlyAvailabilities = new List<AnalyticsHourlyAvailability>();
		}

		public void Delete(Guid personCode, int dateId, int scenarioId)
		{
			var person = personIdAndCode.Where(p => p.Value == personCode).ToList();
			if (!person.Any()) return;

			var personIds = person.Select(p=>p.Key);
			AnalyticsHourlyAvailabilities.RemoveAll(x =>
				personIds.Contains(x.PersonId) && x.DateId == dateId && x.ScenarioId == scenarioId);
		}

		public void AddOrUpdate(AnalyticsHourlyAvailability analyticsHourlyAvailability)
		{
			if (!personIdAndCode.TryGetValue(analyticsHourlyAvailability.PersonId, out var personCode))
			{
				personCode = Guid.NewGuid();
				personIdAndCode[analyticsHourlyAvailability.PersonId] = personCode;
			}
			AnalyticsHourlyAvailabilities.Add(analyticsHourlyAvailability);
		}

		public void UpdateUnlinkedPersonids(int[] personPeriodIds)
		{
			throw new System.NotImplementedException();
		}

		public int GetFactHourlyAvailabilityRowCount(int personPeriodId)
		{
			throw new System.NotImplementedException();
		}
	}
}