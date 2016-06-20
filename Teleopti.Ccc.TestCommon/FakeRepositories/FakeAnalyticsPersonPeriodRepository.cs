using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsPersonPeriodRepository : IAnalyticsPersonPeriodRepository
	{
		private readonly List<AnalyticsPersonPeriod> fakePersonPeriods;
		private readonly List<AnalyticsBridgeAcdLoginPerson> fakeAcdLoginPersons; 

		public FakeAnalyticsPersonPeriodRepository()
		{
			fakePersonPeriods = new List<AnalyticsPersonPeriod>();
			fakeAcdLoginPersons = new List<AnalyticsBridgeAcdLoginPerson>();
		}

		public AnalyticsPersonPeriod PersonPeriod(Guid personPeriodCode)
		{
			return fakePersonPeriods.First(a => a.PersonPeriodCode == personPeriodCode);
		}

		public void AddPersonPeriod(AnalyticsPersonPeriod personPeriod)
		{
			if (personPeriod.PersonId == 0)
			{
				if (fakePersonPeriods.IsEmpty())
					personPeriod.PersonId = 0;
				else
					personPeriod.PersonId = fakePersonPeriods.Max(a => a.PersonId) + 1;
			}
			fakePersonPeriods.Add(personPeriod);
		}

		public void UpdatePersonPeriod(AnalyticsPersonPeriod personPeriod)
		{
			fakePersonPeriods.RemoveAll(a => a.PersonPeriodCode.Equals(personPeriod.PersonPeriodCode));
			fakePersonPeriods.Add(personPeriod);
		}

		public void DeletePersonPeriod(AnalyticsPersonPeriod analyticsPersonPeriod)
		{
			throw new NotImplementedException();
		}

		public IList<AnalyticsBridgeAcdLoginPerson> GetBridgeAcdLoginPersonsForPerson(int personId)
		{
			return fakeAcdLoginPersons.Where(a => a.PersonId == personId).ToList();
		}

		public IList<AnalyticsBridgeAcdLoginPerson> GetBridgeAcdLoginPersonsForAcdLoginPersons(int acdLoginId)
		{
			return fakeAcdLoginPersons.Where(a => a.AcdLoginId == acdLoginId).ToList();
		}

		public void AddBridgeAcdLoginPerson(AnalyticsBridgeAcdLoginPerson bridgeAcdLoginPerson)
		{
			fakeAcdLoginPersons.Add(bridgeAcdLoginPerson);
		}

		public void DeleteBridgeAcdLoginPerson(int acdLoginId, int personId)
		{
			fakeAcdLoginPersons.RemoveAll(a => a.AcdLoginId == acdLoginId && a.PersonId == personId);
		}

		public IList<AnalyticsPersonPeriod> GetPersonPeriods(Guid personCode)
		{
			return fakePersonPeriods.Where(a => a.PersonCode.Equals(personCode)).ToArray();
		}

		public int SiteId(Guid siteCode, string siteName, int businessUnitId)
		{
			return 123;
		}
	}
}
