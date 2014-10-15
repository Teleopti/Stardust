using System;

namespace Teleopti.Interfaces.Domain
{
	public interface IAgentBadgeSettings : ISettingValue
	{
		bool BadgeEnabled { get; set; }
		bool AnsweredCallsBadgeEnabled { get; set; }
		bool AHTBadgeEnabled { get; set; }
		bool AdherenceBadgeEnabled { get; set; }

		int AnsweredCallsThreshold { get; set; }
		TimeSpan AHTThreshold { get; set; }
		Percent AdherenceThreshold { get; set; }
		
		int SilverToBronzeBadgeRate { get; set; }
		int GoldToSilverBadgeRate { get; set; }
	}
}