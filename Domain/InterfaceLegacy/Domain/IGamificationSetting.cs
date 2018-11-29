using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IGamificationSetting : IAggregateRoot, IChangeInfo, IBelongsToBusinessUnit, ICloneableEntity<IGamificationSetting>
	{
		Description Description { get; set; }
		GamificationSettingRuleSet GamificationSettingRuleSet { get; set; }
		bool IsDeleted { get; }
		
		bool AnsweredCallsBadgeEnabled { get; set; }
		bool AHTBadgeEnabled { get; set; }
		bool AdherenceBadgeEnabled { get; set; }
		
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

		IList<IBadgeSetting> BadgeSettings { get; set; }

		int SilverToBronzeBadgeRate { get; set; }
		int GoldToSilverBadgeRate { get; set; }
		GamificationRollingPeriodSet RollingPeriodSet { get; set; }

		void AddBadgeSetting(IBadgeSetting newBadgeSetting);
		IEnumerable<BadgeTypeInfo> EnabledBadgeTypes();
		string GetExternalBadgeTypeName(int badgeType);
	}
}