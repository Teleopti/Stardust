using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Infrastructure.Analytics;

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

		void UpdatePersonNames(CommonNameDescriptionSetting commonNameDescriptionSetting, Guid businessUnitCode);
		IAnalyticsPersonBusinessUnit PersonAndBusinessUnit(Guid personPeriodCode);
	}
}