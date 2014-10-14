﻿using System;

namespace Teleopti.Interfaces.Domain
{
	public interface IAgentBadgeThresholdSettings : ISettingValue
	{
		bool EnableBadge { get; set; }
		int AnsweredCallsThreshold { get; set; }
		TimeSpan AHTThreshold { get; set; }
		Percent AdherenceThreshold { get; set; }
		int SilverToBronzeBadgeRate { get; set; }
		int GoldToSilverBadgeRate { get; set; }
		bool AnsweredCallsBadgeTypeSelected { get; set; }
		bool AHTBadgeTypeSelected { get; set; }
		bool AdherenceBadgeTypeSelected { get; set; }
	}
}