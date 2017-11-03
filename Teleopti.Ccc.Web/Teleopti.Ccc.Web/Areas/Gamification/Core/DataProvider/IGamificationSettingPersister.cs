﻿using System;
using Teleopti.Ccc.Web.Areas.Gamification.Models;

namespace Teleopti.Ccc.Web.Areas.Gamification.Core.DataProvider
{
	public interface IGamificationSettingPersister
	{
		GamificationSettingViewModel Persist();
		bool RemoveGamificationSetting(Guid id);
		bool ResetGamificationSetting();
		GamificationDescriptionViewModel PersistDescription(GamificationDescriptionViewModel input);
		GamificationThresholdEnabledViewModel PersistAnsweredCallsEnabled(GamificationThresholdEnabledViewModel input);
		GamificationAnsweredCallsThresholdViewModel PersistAnsweredCallsGoldThreshold(GamificationAnsweredCallsThresholdViewModel input);
		GamificationAnsweredCallsThresholdViewModel PersistAnsweredCallsSilverThreshold(GamificationAnsweredCallsThresholdViewModel input);
		GamificationAnsweredCallsThresholdViewModel PersistAnsweredCallsBronzeThreshold(GamificationAnsweredCallsThresholdViewModel input);
		GamificationThresholdEnabledViewModel PersistAHTEnabled(GamificationThresholdEnabledViewModel input);
		GamificationAHTThresholdViewModel PersistAHTGoldThreshold(GamificationAHTThresholdViewModel input);
		GamificationAHTThresholdViewModel PersistAHTSilverThreshold(GamificationAHTThresholdViewModel input);
		GamificationAHTThresholdViewModel PersistAHTBronzeThreshold(GamificationAHTThresholdViewModel input);
		GamificationThresholdEnabledViewModel PersistAdherenceEnabled(GamificationThresholdEnabledViewModel input);
		GamificationAdherenceThresholdViewModel PersistAdherenceGoldThreshold(GamificationAdherenceThresholdViewModel input);
		GamificationAdherenceThresholdViewModel PersistAdherenceSilverThreshold(GamificationAdherenceThresholdViewModel input);
		GamificationAdherenceThresholdViewModel PersistAdherenceBronzeThreshold(GamificationAdherenceThresholdViewModel input);
		GamificationSettingViewModel PersistRuleChange(GamificationChangeRuleForm input);
	}
}