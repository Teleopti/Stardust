using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsPersonPeriodRepository
	{
		int SiteId(Guid siteCode, string siteName, int businessUnitId);
		int BusinessUnitId(Guid businessUnitCode);

		IList<IAnalyticsPersonPeriod> GetPersonPeriods(Guid personCode);
		void AddPersonPeriod(IAnalyticsPersonPeriod personPeriod);
		void UpdatePersonPeriod(IAnalyticsPersonPeriod personPeriod);
		int TeamId(Guid teamCode, int siteId, string teamName, int businessUnitId);

		int? SkillSetId(IList<IAnalyticsSkill> skills);
		IList<IAnalyticsSkill> Skills(int businessUnitId);
		int? TimeZone(string timeZoneCode);
		IAnalyticsDate MaxDate();
		IAnalyticsDate MinDate();
		IAnalyticsDate Date(DateTime date);
		int IntervalsPerDay();
		int MaxIntervalId();

	}
}
