using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Gamification.Models;

namespace Teleopti.Ccc.Web.Areas.Gamification.Core.DataProvider
{
	public class GamificationSettingPersister : IGamificationSettingPersister
	{
		private readonly IGamificationSettingRepository _gamificationSettingRepository;

		public GamificationSettingPersister(IGamificationSettingRepository gamificationSettingRepository)
		{
			_gamificationSettingRepository = gamificationSettingRepository;
		}

		public GamificationSettingViewModel Persist()
		{
			var gamificationSetting = getGamificationSetting(null);
			if (!gamificationSetting.Id.HasValue)
			{
				_gamificationSettingRepository.Add(gamificationSetting);
			}

			return new GamificationSettingViewModel(gamificationSetting);
		}

		public GamificationDescriptionViewMode PersistDescription(GamificationDescriptionViewMode input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting.Description != input.Value) gamificationSetting.Description = input.Value;

			return new GamificationDescriptionViewMode(){GamificationSettingId = gamificationSetting.Id.Value, Value = gamificationSetting.Description};
		}

		public GamificationAnsweredCallsEnabledViewModel PersistAnsweredCallsEnabled(GamificationAnsweredCallsEnabledViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting.AnsweredCallsBadgeEnabled != input.Value) gamificationSetting.AnsweredCallsBadgeEnabled = input.Value;

			return new GamificationAnsweredCallsEnabledViewModel(){GamificationSettingId = gamificationSetting.Id.Value, Value = gamificationSetting.AnsweredCallsBadgeEnabled};
		}

		public GamificationAnsweredCallsThresholdViewModel PersistAnsweredCallsGoldThreshold(GamificationAnsweredCallsThresholdViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting.AnsweredCallsGoldThreshold != input.Value) gamificationSetting.AnsweredCallsGoldThreshold = input.Value;

			return new GamificationAnsweredCallsThresholdViewModel(){GamificationSettingId = gamificationSetting.Id.Value, Value = gamificationSetting.AnsweredCallsGoldThreshold};
		}

		public GamificationAnsweredCallsThresholdViewModel PersistAnsweredCallsSilverThreshold(GamificationAnsweredCallsThresholdViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting.AnsweredCallsSilverThreshold != input.Value) gamificationSetting.AnsweredCallsSilverThreshold = input.Value;

			return new GamificationAnsweredCallsThresholdViewModel(){GamificationSettingId = gamificationSetting.Id.Value, Value = gamificationSetting.AnsweredCallsSilverThreshold};
		}

		public GamificationAnsweredCallsThresholdViewModel PersistAnsweredCallsBronzeThreshold(GamificationAnsweredCallsThresholdViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting.AnsweredCallsBronzeThreshold != input.Value) gamificationSetting.AnsweredCallsBronzeThreshold = input.Value;

			return new GamificationAnsweredCallsThresholdViewModel(){GamificationSettingId = gamificationSetting.Id.Value, Value = gamificationSetting.AnsweredCallsBronzeThreshold};
		}

		private IGamificationSetting getGamificationSetting(Guid? id)
		{
			return id == null ? new GamificationSetting(Resources.NewGamificationSetting) : _gamificationSettingRepository.Load((Guid)id);
		}
	}
}