﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Badge
{
	public interface IPushMessageSender
	{
		void SendMessage(IEnumerable<IAgentBadgeWithRankTransaction> agentBadgeWithRankTransactions, string badgeName,
			DateOnly calculateDate);

		void SendMessage(IEnumerable<IAgentBadgeTransaction> agentBadgeTransactions, IBadgeSetting badgeSetting,
			DateOnly calculateDate, IGamificationSetting setting);

		void SendMessagesToPeopleGotABadge(IEnumerable<IAgentBadgeTransaction> newAwardedBadges, IGamificationSetting setting,
			DateOnly calculateDate, int badgeType);

		void SendMessagesToPeopleGotABadge(IEnumerable<IAgentBadgeWithRankTransaction> newAwardedBadges,
			IGamificationSetting setting, DateOnly calculateDate, int badgeType);
	}
}