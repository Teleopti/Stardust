using System;

namespace Teleopti.Interfaces.Domain
{
	public interface IAgentBadgeThresholdSettings 
	{
		int AnsweredCallsThreshold { get; set; }
		TimeSpan AHTThreshold { get; set; }
		Percent AdherenceThreshold { get; set; }
		int SilverBadgeDaysThreshold { get; set; }
		int GoldBadgeDaysThreshold { get; set; }

	}
}