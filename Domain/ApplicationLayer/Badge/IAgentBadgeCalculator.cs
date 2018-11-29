using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Badge
{
	public interface IAgentBadgeCalculator
	{
		IEnumerable<IAgentBadgeTransaction> CalculateAdherenceBadges(IEnumerable<IPerson> allPersons, string timezoneCode,
			DateOnly date, AdherenceReportSettingCalculationMethod adherenceCalculationMethod,
			IGamificationSetting setting, Guid businessUnitId);

		IEnumerable<IAgentBadgeTransaction> CalculateAHTBadges(IEnumerable<IPerson> allPersons, string timezoneCode,
			DateOnly date, IGamificationSetting setting, Guid businessUnitId);

		IEnumerable<IAgentBadgeTransaction> CalculateAnsweredCallsBadges(IEnumerable<IPerson> allPersons, string timezoneCode,
			DateOnly date, IGamificationSetting setting, Guid businessUnitId);

		IEnumerable<IAgentBadgeTransaction> CalculateBadges(IEnumerable<IPerson> allPersons, DateOnly date,
			IBadgeSetting badgeSetting, Guid businessId);

		void ResetAgentBadges();
		void RemoveAgentBadges(DateOnlyPeriod period);
	}
}