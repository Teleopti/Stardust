using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsPreferenceRepository : IAnalyticsPreferenceRepository
	{
		private readonly Dictionary<int, Guid> dimPerson = new Dictionary<int, Guid>();
		private readonly List<AnalyticsFactSchedulePreference> fakeAnalyticsFactSchedulePreferences;

		public FakeAnalyticsPreferenceRepository()
		{
			fakeAnalyticsFactSchedulePreferences = new List<AnalyticsFactSchedulePreference>();
		}

		public void AddDimPerson(int personId, Guid personCode)
		{
			if (dimPerson.ContainsKey(personId))
			{
				throw new ApplicationException($"Fake dim person with personId {personId} already exists");
			}

			dimPerson[personId] = personCode;
		}

		public void AddPreference(AnalyticsFactSchedulePreference analyticsFactSchedulePreference)
		{
			if (!dimPerson.TryGetValue(analyticsFactSchedulePreference.PersonId, out var personCode))
			{
				personCode = Guid.NewGuid();
				dimPerson[analyticsFactSchedulePreference.PersonId] = personCode;
			}

			fakeAnalyticsFactSchedulePreferences.Add(analyticsFactSchedulePreference);
		}

		public void DeletePreferences(int dateId, Guid personCode, int? scenarioId = null)
		{
			var person = dimPerson.Where(p => p.Value == personCode).ToList();
			if (!person.Any()) return;

			var personIds = person.Select(p => p.Key);

			if (scenarioId == null)
				fakeAnalyticsFactSchedulePreferences.RemoveAll(a => a.DateId == dateId && personIds.Contains(a.PersonId));
			else
				fakeAnalyticsFactSchedulePreferences.RemoveAll(a => a.DateId == dateId && personIds.Contains(a.PersonId) && a.ScenarioId == scenarioId);
		}

		public IList<AnalyticsFactSchedulePreference> PreferencesForPerson(int personId)
		{
			return fakeAnalyticsFactSchedulePreferences.Where(a => a.PersonId == personId).ToList();
		}

		public void UpdateUnlinkedPersonids(int[] personPeriodIds)
		{
			throw new System.NotImplementedException();
		}
	}
}