using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.Analytics;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsPersonPeriodRepository
	{
		int SiteId(Guid siteCode, string siteName, int businessUnitId);
		int BusinessUnitId(Guid businessUnitCode);

		IList<AnalyticsPersonPeriod> GetPersonPeriods(Guid personCode);
		void AddPersonPeriod(AnalyticsPersonPeriod personPeriod);
		void UpdatePersonPeriod(AnalyticsPersonPeriod personPeriod);
		int TeamId(Guid teamCode, int siteId, string teamName, int businessUnitId);

		int? SkillSetId(IList<AnalyticsSkill> skills);
		IList<AnalyticsSkill> Skills(int businessUnitId);
		int? TimeZone(string timeZoneCode);
		IAnalyticsDate MaxDate();
		IAnalyticsDate MinDate();
		IAnalyticsDate Date(DateTime date);
		int IntervalsPerDay();
		int MaxIntervalId();
		void DeletePersonPeriod(AnalyticsPersonPeriod analyticsPersonPeriod);
	}
}