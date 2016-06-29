using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsPersonPeriodRepository
	{
		int GetOrCreateSite(Guid siteCode, string siteName, int businessUnitId);

		IList<AnalyticsPersonPeriod> GetPersonPeriods(Guid personCode);
		AnalyticsPersonPeriod PersonPeriod(Guid personPeriodCode);
		void AddPersonPeriod(AnalyticsPersonPeriod personPeriod);
		void UpdatePersonPeriod(AnalyticsPersonPeriod personPeriod);
	
		void DeletePersonPeriod(AnalyticsPersonPeriod analyticsPersonPeriod);

		IList<AnalyticsBridgeAcdLoginPerson> GetBridgeAcdLoginPersonsForPerson(int personId);
		IList<AnalyticsBridgeAcdLoginPerson> GetBridgeAcdLoginPersonsForAcdLoginPersons(int acdLoginId);
		void AddBridgeAcdLoginPerson(AnalyticsBridgeAcdLoginPerson bridgeAcdLoginPerson);
		void DeleteBridgeAcdLoginPerson(int acdLoginId, int personId);
	}
}