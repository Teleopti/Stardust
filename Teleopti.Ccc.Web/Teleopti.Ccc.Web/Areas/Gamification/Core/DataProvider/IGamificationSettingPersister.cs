using System;
using Teleopti.Ccc.Web.Areas.Gamification.Models;

namespace Teleopti.Ccc.Web.Areas.Gamification.Core.DataProvider
{
	public interface IGamificationSettingPersister
	{
		GamificationSettingViewModel Persist();
		GamificationDescriptionViewMode PersistDescription(GamificationDescriptionViewMode input);
		GamificationAnsweredCallsEnabledViewModel PersistAnsweredCallsEnabled(GamificationAnsweredCallsEnabledViewModel input);
		GamificationAnsweredCallsThresholdViewModel PersistAnsweredCallsGoldThreshold(GamificationAnsweredCallsThresholdViewModel input);
		GamificationAnsweredCallsThresholdViewModel PersistAnsweredCallsSilverThreshold(GamificationAnsweredCallsThresholdViewModel input);
		GamificationAnsweredCallsThresholdViewModel PersistAnsweredCallsBronzeThreshold(GamificationAnsweredCallsThresholdViewModel input);
	}
}