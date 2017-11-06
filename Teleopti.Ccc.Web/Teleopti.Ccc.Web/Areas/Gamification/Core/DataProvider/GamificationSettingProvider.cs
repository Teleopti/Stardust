using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Gamification.Mapping;
using Teleopti.Ccc.Web.Areas.Gamification.Models;

namespace Teleopti.Ccc.Web.Areas.Gamification.Core.DataProvider
{
	public class GamificationSettingProvider : IGamificationSettingProvider
	{
		private readonly IGamificationSettingRepository _gamificationSettingRepository;
		private readonly IGamificationSettingMapper _mapper;

		public GamificationSettingProvider(IGamificationSettingRepository gamificationSettingRepository, IGamificationSettingMapper mapper)
		{
			_gamificationSettingRepository = gamificationSettingRepository;
			_mapper = mapper;
		}

		public GamificationSettingViewModel GetGamificationSetting(Guid id)
		{
			var setting = _gamificationSettingRepository.Get(id);
			return setting == null ? null : _mapper.Map(setting);
		}

		public IList<GamificationDescriptionViewModel> GetGamificationList()
		{
			var settings = _gamificationSettingRepository.LoadAll();

			return settings.Select(setting => new GamificationDescriptionViewModel()
					{
						GamificationSettingId = setting.Id.Value,
						Value = setting.Description
					}).ToList();
		}
	}
}