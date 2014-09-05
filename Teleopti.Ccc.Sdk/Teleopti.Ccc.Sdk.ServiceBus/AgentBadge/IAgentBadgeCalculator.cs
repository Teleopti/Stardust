using System.Collections.Generic;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.AgentBadge
{
	public interface IAgentBadgeCalculator
	{
		IEnumerable<IAgentBadgeTransaction> CalculateAdherenceBadges(IEnumerable<IPerson> allPersons, string timezoneCode,
			DateOnly date, AdherenceReportSettingCalculationMethod adherenceCalculationMethod,
			IAgentBadgeThresholdSettings setting);

		IEnumerable<IAgentBadgeTransaction> CalculateAHTBadges(IEnumerable<IPerson> allPersons, string timezoneCode,
			DateOnly date, IAgentBadgeThresholdSettings setting);

		IEnumerable<IAgentBadgeTransaction> CalculateAnsweredCallsBadges(IEnumerable<IPerson> allPersons, string timezoneCode,
			DateOnly date, IAgentBadgeThresholdSettings setting);
	}
}