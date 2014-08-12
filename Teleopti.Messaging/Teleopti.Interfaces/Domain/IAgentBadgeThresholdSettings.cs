using System;

namespace Teleopti.Interfaces.Domain
{
	public interface IAgentBadgeThresholdSettings : IAggregateRoot
	{
		bool EnableBadge { get; set; }
		int AnsweredCallsThreshold { get; set; }
		TimeSpan AHTThreshold { get; set; }
		Percent AdherenceThreshold { get; set; }
		int SilverToBronzeBadgeRate { get; set; }
		int GoldToSilverBadgeRate { get; set; }
	}
}