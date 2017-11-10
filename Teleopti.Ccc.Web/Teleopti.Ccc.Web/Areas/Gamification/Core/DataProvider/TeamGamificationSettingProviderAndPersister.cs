using System;
using System.Collections.Generic;
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
			var VMList = new List<TeamGamificationSettingViewModel>();
			var teams = _teamRepository.FindTeams(teamIds);

			foreach (var team in teams)
			{
				var teamGamificationSetting = _teamGamificationSettingRepository.FindTeamGamificationSettingsByTeam(team);

				var gamificationSettingId = Guid.Empty;
				if (teamGamificationSetting != null) gamificationSettingId = teamGamificationSetting.GamificationSetting.Id.Value;

				VMList.Add(new TeamGamificationSettingViewModel()
				{
					Team = new SelectOptionItem()
					{
						id = team.Id.Value.ToString(),
						text = team.Description.Name
					},
					GamificationSettingId = gamificationSettingId
				});
			}

			return VMList;
		}

		public TeamGamificationSettingViewModel SetTeamGamificationSetting(TeamGamificationSettingForm input)
		{
			var team = _teamRepository.Get(input.TeamId);
			if (team == null ) return null;

			var gamificationSettingId = Guid.Empty;
			var teamGamificationSetting = _teamGamificationSettingRepository.FindTeamGamificationSettingsByTeam(team);
			if (input.GamificationSettingId == Guid.Empty)
			{
				if (teamGamificationSetting != null)
				{
					_teamGamificationSettingRepository.Remove(teamGamificationSetting);
				}
			}
			else
			{
				var gamificationSetting = _gamificationSettingRepository.Get(input.GamificationSettingId);
				if (gamificationSetting == null) return null;
				
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

				gamificationSettingId = teamGamificationSetting.GamificationSetting.Id.Value;
			}

			return new TeamGamificationSettingViewModel()
			{
				GamificationSettingId = gamificationSettingId,
				Team = new SelectOptionItem() { id = team.Id.ToString(), text = team.Description.Name }
			};
		}
	}
}