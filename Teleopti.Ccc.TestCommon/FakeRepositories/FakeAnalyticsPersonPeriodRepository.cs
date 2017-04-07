using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;

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
			fakePersonPeriods.RemoveAll(a => a.PersonId.Equals(personPeriod.PersonId));
			fakePersonPeriods.Add(personPeriod);
		}

		public void DeletePersonPeriod(AnalyticsPersonPeriod analyticsPersonPeriod)
		{
			fakePersonPeriods.RemoveAll(x => x.PersonCode == analyticsPersonPeriod.PersonCode);
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

		public void UpdatePersonNames(CommonNameDescriptionSetting commonNameDescriptionSetting, Guid businessUnitCode)
		{
			fakePersonPeriods
				.Where(a => a.BusinessUnitCode == businessUnitCode)
				.ForEach(a => a.PersonName = commonNameDescriptionSetting.BuildCommonNameDescription(a.FirstName, a.LastName, a.EmploymentNumber));
		}

		public IList<AnalyticsPersonPeriod> GetPersonPeriods(Guid personCode)
		{
			return fakePersonPeriods.Where(a => a.PersonCode.Equals(personCode)).ToArray();
		}

		public int GetOrCreateSite(Guid siteCode, string siteName, int businessUnitId)
		{
			return 123;
		}

		public IAnalyticsPersonBusinessUnit PersonAndBusinessUnit(Guid personPeriodCode)
		{
			var personPeriod = fakePersonPeriods.SingleOrDefault(x => x.PersonPeriodCode == personPeriodCode);
			return personPeriod == null ? null : new AnalyticsPersonBusinessUnit {BusinessUnitId = personPeriod.BusinessUnitId, PersonId = personPeriod.PersonId};
		}

		public void UpdateValidToLocalDateIds(IAnalyticsDate maxDate)
		{
			foreach (var period in fakePersonPeriods.Where(p => p.ValidToDateId == -2 && p.PersonId >= 0))
			{
				period.ValidToDateIdLocal = maxDate.DateId;
				period.ValidToDateLocal = maxDate.DateDate;
			}
		}
	}
}
