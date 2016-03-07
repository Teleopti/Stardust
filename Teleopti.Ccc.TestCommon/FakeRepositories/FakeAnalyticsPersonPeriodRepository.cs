using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsPersonPeriodRepository : IAnalyticsPersonPeriodRepository
	{
		public readonly DateTime SetupStartDate;
		public readonly DateTime SetupEndDate;
		readonly List<AnalyticsDate> fakeDates = new List<AnalyticsDate>();
		private List<KeyValuePair<int, string>> fakeSkillSets;
		private List<AnalyticsSkill> fakeSkills;
		private readonly List<AnalyticsPersonPeriod> fakePersonPeriods;

		public FakeAnalyticsPersonPeriodRepository(DateTime setupStartDate, DateTime setupEndDate)
		{
			SetupStartDate = setupStartDate;
			SetupEndDate = setupEndDate;

			fakePersonPeriods = new List<AnalyticsPersonPeriod>();

			initDates();
		}

		private void initDates()
		{
			fakeDates.Add(new AnalyticsDate
			{
				DateId = -1,
				DateDate = new DateTime(1900, 01, 01)
			});
			fakeDates.Add(new AnalyticsDate
			{
				DateId = -2,
				DateDate = new DateTime(2059, 12, 31)
			});

			var d = SetupStartDate;
			var dIndex = 0;
			while (d <= SetupEndDate)
			{
				fakeDates.Add(new AnalyticsDate() { DateId = dIndex, DateDate = d });
				d = d.AddDays(1);
				dIndex++;
			}
		}


		public void AddPersonPeriod(AnalyticsPersonPeriod personPeriod)
		{
			fakePersonPeriods.Add(personPeriod);
		}

		public void UpdatePersonPeriod(AnalyticsPersonPeriod personPeriod)
		{
			fakePersonPeriods.RemoveAll(a => a.PersonPeriodCode.Equals(personPeriod.PersonPeriodCode));
			fakePersonPeriods.Add(personPeriod);
		}

		public int BusinessUnitId(Guid businessUnitCode)
		{
			return 1;
		}

		public IAnalyticsDate Date(DateTime date)
		{
			return fakeDates.FirstOrDefault(a => a.DateDate.Equals(date));
		}

		public int IntervalsPerDay()
		{
			return 96;
		}

		public int MaxIntervalId()
		{
			return 95;
		}

		public void DeletePersonPeriod(AnalyticsPersonPeriod analyticsPersonPeriod)
		{
			throw new NotImplementedException();
		}

		public IList<AnalyticsPersonPeriod> GetPersonPeriods(Guid personCode)
		{
			return fakePersonPeriods.Where(a => a.PersonCode.Equals(personCode)).ToArray();
		}

		public int SiteId(Guid siteCode, string siteName, int businessUnitId)
		{
			return 123;
		}

		public IList<AnalyticsSkill> Skills(int businessUnitId)
		{
			return fakeSkills;
		}

		public int? SkillSetId(IList<AnalyticsSkill> skills)
		{
			var skillSet = fakeSkillSets.FirstOrDefault(a => a.Value == string.Join(",", skills.Select(b => b.SkillId)));
			if (!skillSet.Equals(default(KeyValuePair<int, string>)))
				return skillSet.Key;
			return null;
		}

		public int TeamId(Guid teamCode, int siteId, string teamName, int businessUnitId)
		{
			return 456;
		}

		public int? TimeZone(string timeZoneCode)
		{
			return 1;
		}

		public IAnalyticsDate MaxDate()
		{
			return fakeDates.Last();
		}

		public IAnalyticsDate MinDate()
		{
			return fakeDates.First(a => a.DateId >= 0);
		}

		public void SetSkills(List<AnalyticsSkill> analyticsSkills)
		{
			fakeSkills = analyticsSkills;
		}

		public void SetSkillSets(List<KeyValuePair<int, string>> list)
		{
			fakeSkillSets = list;
		}
	}
}
