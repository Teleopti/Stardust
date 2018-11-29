using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Badge
{
	public interface IAgentBadgeWithRankCalculator
	{
		IEnumerable<IAgentBadgeWithRankTransaction> CalculateAdherenceBadges(IEnumerable<IPerson> allPersons,
			string timezoneCode, DateOnly date, AdherenceReportSettingCalculationMethod adherenceCalculationMethod,
			IGamificationSetting setting, Guid businessUnitId);

		IEnumerable<IAgentBadgeWithRankTransaction> CalculateAHTBadges(IEnumerable<IPerson> allPersons, string timezoneCode,
			DateOnly date, IGamificationSetting setting, Guid businessUnitId);

		IEnumerable<IAgentBadgeWithRankTransaction> CalculateAnsweredCallsBadges(IEnumerable<IPerson> allPersons,
			string timezoneCode, DateOnly date, IGamificationSetting setting, Guid businessUnitId);

		IEnumerable<IAgentBadgeWithRankTransaction> CalculateBadges(IEnumerable<IPerson> allPersons, DateOnly date,
			IBadgeSetting badgeSetting, Guid businessId);

		void ResetAgentBadges();
		void RemoveAgentBadges(DateOnlyPeriod period);
	}
}