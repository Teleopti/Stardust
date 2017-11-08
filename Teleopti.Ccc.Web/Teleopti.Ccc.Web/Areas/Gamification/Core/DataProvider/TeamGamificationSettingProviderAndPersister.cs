using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Gamification.Models;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;

namespace Teleopti.Ccc.Web.Areas.Gamification.Core.DataProvider
{
	public class TeamGamificationSettingProviderAndPersister : ITeamGamificationSettingProviderAndPersister
	{
		private readonly ITeamGamificationSettingRepository _teamGamificationSettingRepository;
		private readonly ITeamRepository _teamRepository;
		private readonly IGamificationSettingRepository _gamificationSettingRepository;

		public TeamGamificationSettingProviderAndPersister(ITeamGamificationSettingRepository teamGamificationSettingRepository, ITeamRepository teamRepository, IGamificationSettingRepository gamificationSettingRepository)
		{
			_teamGamificationSettingRepository = teamGamificationSettingRepository;
			_teamRepository = teamRepository;
			_gamificationSettingRepository = gamificationSettingRepository;
		}

		public IList<TeamGamificationSettingViewModel> GetTeamGamificationSettingViewModels(List<Guid> teamIds)
		{
			var teams = _teamRepository.FindTeams(teamIds);

			return teams.Select(team => _teamGamificationSettingRepository.FindTeamGamificationSettingsByTeam(team)).Select(teamGamificationSetting => new TeamGamificationSettingViewModel()
			{
				Team = new SelectOptionItem()
				{
					id = teamGamificationSetting.Team.Id.Value.ToString(),
					text = teamGamificationSetting.Team.Description.Name
				},
				GamificationSettingId = teamGamificationSetting.GamificationSetting.Id.Value
			}).ToList();
		}

		public TeamGamificationSettingViewModel SetTeamGamificationSetting(TeamGamificationSettingForm input)
		{
			var team = _teamRepository.Get(input.TeamId);
			var gamificationSetting = _gamificationSettingRepository.Get(input.GamificationSettingId);
			if (team == null || gamificationSetting == null) return null;

			var teamGamificationSetting = _teamGamificationSettingRepository.FindTeamGamificationSettingsByTeam(team);
			if (teamGamificationSetting == null)
			{
				
				teamGamificationSetting = new TeamGamificationSetting
				{
					GamificationSetting = gamificationSetting,
					Team = team
				};
				_teamGamificationSettingRepository.Add(teamGamificationSetting);
			}
			else
			{
				teamGamificationSetting.GamificationSetting = gamificationSetting;
			}

			return new TeamGamificationSettingViewModel()
			{
				GamificationSettingId = teamGamificationSetting.GamificationSetting.Id.Value,
				Team = new SelectOptionItem(){id = teamGamificationSetting.Team.Id.ToString(),text = teamGamificationSetting.Team.Description.Name}
			};
		}
	}
}