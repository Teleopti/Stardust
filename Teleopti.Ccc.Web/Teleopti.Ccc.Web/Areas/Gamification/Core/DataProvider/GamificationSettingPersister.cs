﻿using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Gamification.Models;

namespace Teleopti.Ccc.Web.Areas.Gamification.core.DataProvider
{
	public class GamificationSettingPersister : IGamificationSettingPersister
	{
		private readonly IGamificationSettingRepository _gamificationSettingRepository;

		public GamificationSettingPersister(IGamificationSettingRepository gamificationSettingRepository)
		{
			_gamificationSettingRepository = gamificationSettingRepository;
		}

		public GamificationViewModel Persist()
		{
			var gamificationSetting = getGamificationSetting(null);
			if (!gamificationSetting.Id.HasValue)
			{
				_gamificationSettingRepository.Add(gamificationSetting);
			}

			return new GamificationViewModel(gamificationSetting);
		}

		private IGamificationSetting getGamificationSetting(Guid? id)
		{
			return id == null ? new GamificationSetting("Default") : _gamificationSettingRepository.Get((Guid)id);
		}
	}
}