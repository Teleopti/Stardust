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

			return new GamificationSettingViewModel(gamificationSetting) {Id = gamificationSetting.Id.GetValueOrDefault()};
		}

		public GamificationDescriptionViewMode PersistDescription(GamificationDescriptionViewMode input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting.Description != input.Value) gamificationSetting.Description = input.Value;

			return new GamificationDescriptionViewMode(){GamificationSettingId = gamificationSetting.Id.Value, Value = gamificationSetting.Description};
		}

		private IGamificationSetting getGamificationSetting(Guid? id)
		{
			return id == null ? new GamificationSetting(Resources.NewGamificationSetting) : _gamificationSettingRepository.Load((Guid)id);
		}
	}
}