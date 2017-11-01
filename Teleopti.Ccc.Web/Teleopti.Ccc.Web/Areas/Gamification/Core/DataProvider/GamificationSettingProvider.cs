using System;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Gamification.Models;

namespace Teleopti.Ccc.Web.Areas.Gamification.Core.DataProvider
{
	public class GamificationSettingProvider : IGamificationSettingProvider
	{
		private readonly IGamificationSettingRepository _gamificationSettingRepository;

		public GamificationSettingProvider(IGamificationSettingRepository gamificationSettingRepository)
		{
			_gamificationSettingRepository = gamificationSettingRepository;
		}

		public GamificationSettingViewModel GetGamificationSetting(Guid id)
		{
			return new GamificationSettingViewModel(_gamificationSettingRepository.Load(id)) {Id = id};
		}
	}
}