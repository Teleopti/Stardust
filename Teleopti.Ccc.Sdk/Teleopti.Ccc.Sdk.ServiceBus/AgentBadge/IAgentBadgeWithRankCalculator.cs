using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.AgentBadge
{
	public interface IAgentBadgeWithRankCalculator
	{
		IEnumerable<IAgentBadgeWithRankTransaction> CalculateAdherenceBadges(IEnumerable<IPerson> allPersons, string timezoneCode, DateOnly date, AdherenceReportSettingCalculationMethod adherenceCalculationMethod, IAgentBadgeSettings setting, Guid businessUnitId);

		IEnumerable<IAgentBadgeWithRankTransaction> CalculateAHTBadges(IEnumerable<IPerson> allPersons, string timezoneCode, DateOnly date, IAgentBadgeSettings setting, Guid businessUnitId);

		IEnumerable<IAgentBadgeWithRankTransaction> CalculateAnsweredCallsBadges(IEnumerable<IPerson> allPersons, string timezoneCode, DateOnly date, IAgentBadgeSettings setting, Guid businessUnitId);

		IEnumerable<IAgentBadgeWithRankTransaction> CalculateAdherenceBadges(IEnumerable<IPerson> allPersons,
			string timezoneCode, DateOnly date, AdherenceReportSettingCalculationMethod adherenceCalculationMethod,
			IGamificationSetting setting, Guid businessUnitId);

		IEnumerable<IAgentBadgeWithRankTransaction> CalculateAHTBadges(IEnumerable<IPerson> allPersons, string timezoneCode,
			DateOnly date, IGamificationSetting setting, Guid businessUnitId);

		IEnumerable<IAgentBadgeWithRankTransaction> CalculateAnsweredCallsBadges(IEnumerable<IPerson> allPersons,
			string timezoneCode, DateOnly date, IGamificationSetting setting, Guid businessUnitId);
	}
}