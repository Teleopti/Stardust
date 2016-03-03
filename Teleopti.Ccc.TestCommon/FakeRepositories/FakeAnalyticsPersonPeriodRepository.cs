using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
		private List<IAnalyticsSkill> fakeSkills;

		public FakeAnalyticsPersonPeriodRepository(DateTime setupStartDate, DateTime setupEndDate)
		{
			this.SetupStartDate = setupStartDate;
			this.SetupEndDate = setupEndDate;

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


		public void AddPersonPeriod(IAnalyticsPersonPeriod personPeriod)
		{
			throw new NotImplementedException();
		}

		public void UpdatePersonPeriod(IAnalyticsPersonPeriod personPeriod)
		{
			throw new NotImplementedException();
		}

		public int BusinessUnitId(Guid businessUnitCode)
		{
			throw new NotImplementedException();
		}

		public IAnalyticsDate Date(DateTime date)
		{
			return fakeDates.FirstOrDefault(a => a.DateDate.Equals(date));
		}

		public int IntervalsPerDay()
		{
			throw new NotImplementedException();
		}

		public int MaxIntervalId()
		{
			throw new NotImplementedException();
		}

		public IList<IAnalyticsPersonPeriod> GetPersonPeriods(Guid personCode)
		{
			throw new NotImplementedException();
		}

		public int SiteId(Guid siteCode, string siteName, int businessUnitId)
		{
			return 123;
		}

		public IList<IAnalyticsSkill> Skills(int businessUnitId)
		{
			return fakeSkills;
		}

		public int? SkillSetId(IList<IAnalyticsSkill> skills)
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
			return fakeDates.First();
		}

		public void SetSkills(List<IAnalyticsSkill> analyticsSkills)
		{
			fakeSkills = analyticsSkills;
		}

		public void SetSkillSets(List<KeyValuePair<int, string>> list)
		{
			fakeSkillSets = list;
		}
	}
}
