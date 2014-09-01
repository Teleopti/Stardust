using System.Collections.Generic;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.AgentBadge
{
	public interface IAgentBadgeCalculator
	{
		IEnumerable<IAgentBadge> CalculateAdherenceBadges(IEnumerable<IPerson> allPersons, string timezoneCode,
			DateOnly date, AdherenceReportSettingCalculationMethod adherenceCalculationMethod,
			IAgentBadgeThresholdSettings setting);

		IEnumerable<IAgentBadge> CalculateAHTBadges(IEnumerable<IPerson> allPersons, string timezoneCode,
			DateOnly date, IAgentBadgeThresholdSettings setting);

		IEnumerable<IAgentBadge> CalculateAnsweredCallsBadges(IEnumerable<IPerson> allPersons, string timezoneCode,
			DateOnly date, IAgentBadgeThresholdSettings setting);
	}
}