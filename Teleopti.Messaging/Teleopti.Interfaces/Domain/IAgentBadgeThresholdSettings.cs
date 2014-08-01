using System;

namespace Teleopti.Interfaces.Domain
{
	public interface IAgentBadgeThresholdSettings 
	{
		bool EnableBadge { get; set; }
		int AnsweredCallsThreshold { get; set; }
		TimeSpan AHTThreshold { get; set; }
		Percent AdherenceThreshold { get; set; }
		int SilverBadgeDaysThreshold { get; set; }
		int GoldBadgeDaysThreshold { get; set; }

	}
}