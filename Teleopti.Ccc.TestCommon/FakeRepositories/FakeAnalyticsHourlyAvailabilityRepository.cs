using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsHourlyAvailabilityRepository : IAnalyticsHourlyAvailabilityRepository
	{
		private readonly Dictionary<int, Guid> dimPerson = new Dictionary<int, Guid>();
		public readonly List<AnalyticsHourlyAvailability> AnalyticsHourlyAvailabilities;

		public FakeAnalyticsHourlyAvailabilityRepository()
		{
			AnalyticsHourlyAvailabilities = new List<AnalyticsHourlyAvailability>();
		}

		public void AddDimPerson(int personId, Guid personCode)
		{
			if (dimPerson.ContainsKey(personId))
			{
				throw new ApplicationException($"Fake dim person with personId {personId} already exists");
			}

			dimPerson[personId] = personCode;
		}

		public void Delete(Guid personCode, int dateId, int scenarioId)
		{
			var person = dimPerson.Where(p => p.Value == personCode).ToList();
			if (!person.Any()) return;

			var personIds = person.Select(p=>p.Key);
			AnalyticsHourlyAvailabilities.RemoveAll(x =>
				personIds.Contains(x.PersonId) && x.DateId == dateId && x.ScenarioId == scenarioId);
		}

		public void AddOrUpdate(AnalyticsHourlyAvailability analyticsHourlyAvailability)
		{
			if (!dimPerson.TryGetValue(analyticsHourlyAvailability.PersonId, out var personCode))
			{
				personCode = Guid.NewGuid();
				dimPerson[analyticsHourlyAvailability.PersonId] = personCode;
			}
			AnalyticsHourlyAvailabilities.Add(analyticsHourlyAvailability);
		}

		public void UpdateUnlinkedPersonids(int[] personPeriodIds)
		{
			throw new System.NotImplementedException();
		}
	}
}