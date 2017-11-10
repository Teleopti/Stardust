using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.Gamification.Models;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Gamification.Core.DataProvider
{
	public class TeamGamificationSettingProviderAndPersister : ITeamGamificationSettingProviderAndPersister
	{
		private readonly ITeamGamificationSettingRepository _teamGamificationSettingRepository;
		private readonly ITeamRepository _teamRepository;
		private readonly IGamificationSettingRepository _gamificationSettingRepository;
		private readonly ISiteProvider _siteProvider;

		public TeamGamificationSettingProviderAndPersister(ITeamGamificationSettingRepository teamGamificationSettingRepository, 
			ITeamRepository teamRepository, IGamificationSettingRepository gamificationSettingRepository, ISiteProvider siteProvider)
		{
			_teamGamificationSettingRepository = teamGamificationSettingRepository;
			_teamRepository = teamRepository;
			_gamificationSettingRepository = gamificationSettingRepository;
			_siteProvider = siteProvider;
		}

		public IList<TeamGamificationSettingViewModel> GetTeamGamificationSettingViewModels(List<Guid> siteIds)
		{
			var teamIds = getTeamIds(siteIds);

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

		private IList<Guid> getTeamIds(List<Guid> siteIds)
		{
			var ids = new List<Guid>();
			foreach (var siteId in siteIds)
			{
				var teams = _siteProvider.GetPermittedTeamsUnderSite(siteId, DateOnly.Today, DefinedRaptorApplicationFunctionPaths.OpenOptionsPage).ToList();
				ids.AddRange(teams.Select(team => team.Id.Value));
			}

			return ids;
		}
	}
}