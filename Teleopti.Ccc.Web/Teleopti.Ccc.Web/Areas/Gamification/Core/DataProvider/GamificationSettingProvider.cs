using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Gamification.Mapping;
using Teleopti.Ccc.Web.Areas.Gamification.Models;


namespace Teleopti.Ccc.Web.Areas.Gamification.Core.DataProvider
{
	public class GamificationSettingProvider : IGamificationSettingProvider
	{
		private readonly IGamificationSettingRepository _gamificationSettingRepository;
		private readonly IGamificationSettingMapper _mapper;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly ITeamGamificationSettingRepository _teamGamificationSettingRepository;

		public GamificationSettingProvider(IGamificationSettingRepository gamificationSettingRepository, IGamificationSettingMapper mapper, ILoggedOnUser loggedOnUser, ITeamGamificationSettingRepository teamGamificationSettingRepository)
		{
			_gamificationSettingRepository = gamificationSettingRepository;
			_mapper = mapper;
			_loggedOnUser = loggedOnUser;
			_teamGamificationSettingRepository = teamGamificationSettingRepository;
		}

		public GamificationSettingViewModel GetGamificationSetting(Guid id)
		{
			var setting = _gamificationSettingRepository.Get(id);

			if (setting == null)
			{
				return default(GamificationSettingViewModel);
			}
			else {
				var result = _mapper.Map(setting);
				result.ExternalBadgeSettings = result.ExternalBadgeSettings.OrderBy(ebs => ebs.QualityId).ToList();
				return result;
			}
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
		
		public IGamificationSetting GetGamificationSetting()
		{
			var person = _loggedOnUser.CurrentUser();
			var myTeam = person.MyTeam(DateOnly.Today);
			if (myTeam == null) return null;

			var teamGamificationSetting = _teamGamificationSettingRepository.FindTeamGamificationSettingsByTeam(myTeam);
			return teamGamificationSetting?.GamificationSetting;
		}
	}
}