﻿using System;

namespace Teleopti.Interfaces.Domain
{
	public interface IAgentBadgeThresholdSettings : IAggregateRoot
	{
		bool EnableBadge { get; set; }
		TimeSpan CalculationTime { get; set; }
		int AnsweredCallsThreshold { get; set; }
		TimeSpan AHTThreshold { get; set; }
		Percent AdherenceThreshold { get; set; }
		int SilverBadgeDaysThreshold { get; set; }
		int GoldBadgeDaysThreshold { get; set; }
	}
}