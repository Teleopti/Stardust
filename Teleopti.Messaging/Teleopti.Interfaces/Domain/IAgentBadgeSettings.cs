using System;

namespace Teleopti.Interfaces.Domain
{
	public interface IAgentBadgeSettings : ISettingValue
	{
		bool BadgeEnabled { get; set; }
		bool AnsweredCallsBadgeEnabled { get; set; }
		bool AHTBadgeEnabled { get; set; }
		bool AdherenceBadgeEnabled { get; set; }

		bool EnableDifferentLevelBadgeCalculation { get; set; }

		int AnsweredCallsThreshold { get; set; }
		int AnsweredCallsBronzeThreshold { get; set; }
		int AnsweredCallsSilverThreshold { get; set; }
		int AnsweredCallsGoldThreshold { get; set; }

		TimeSpan AHTThreshold { get; set; }
		TimeSpan AHTBronzeThreshold { get; set; }
		TimeSpan AHTSilverThreshold { get; set; }
		TimeSpan AHTGoldThreshold { get; set; }

		Percent AdherenceThreshold { get; set; }
		Percent AdherenceBronzeThreshold { get; set; }
		Percent AdherenceSilverThreshold { get; set; }
		Percent AdherenceGoldThreshold { get; set; }
		
		int SilverToBronzeBadgeRate { get; set; }
		int GoldToSilverBadgeRate { get; set; }
	}
}