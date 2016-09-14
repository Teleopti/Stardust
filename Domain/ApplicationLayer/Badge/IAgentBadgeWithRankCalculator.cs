using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Badge
{
	public interface IAgentBadgeWithRankCalculator
	{
		IEnumerable<IAgentBadgeWithRankTransaction> CalculateAdherenceBadges(IEnumerable<IPerson> allPersons,
			string timezoneCode, DateOnly date, AdherenceReportSettingCalculationMethod adherenceCalculationMethod,
			IGamificationSetting setting, Guid businessUnitId, int? timeoutInSecond = null);

		IEnumerable<IAgentBadgeWithRankTransaction> CalculateAHTBadges(IEnumerable<IPerson> allPersons, string timezoneCode,
			DateOnly date, IGamificationSetting setting, Guid businessUnitId, int? timeoutInSecond = null);

		IEnumerable<IAgentBadgeWithRankTransaction> CalculateAnsweredCallsBadges(IEnumerable<IPerson> allPersons,
			string timezoneCode, DateOnly date, IGamificationSetting setting, Guid businessUnitId, int? timeoutInSecond = null);
	}
}