using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.Gamification.Models;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;


namespace Teleopti.Ccc.Web.Areas.Gamification.Core.DataProvider
{
	public class TeamGamificationSettingProviderAndPersister : ITeamGamificationSettingProviderAndPersister
	{
		private readonly ITeamGamificationSettingRepository _teamGamificationSettingRepository;
		private readonly ITeamRepository _teamRepository;
		private readonly IGamificationSettingRepository _gamificationSettingRepository;
		private readonly IPermissionProvider _permissionProvider;
		const string functionPath = DefinedRaptorApplicationFunctionPaths.Gamification;

		public TeamGamificationSettingProviderAndPersister(ITeamGamificationSettingRepository teamGamificationSettingRepository,
			ITeamRepository teamRepository, IGamificationSettingRepository gamificationSettingRepository, IPermissionProvider permissionProvider)
		{
			_teamGamificationSettingRepository = teamGamificationSettingRepository;
			_teamRepository = teamRepository;
			_gamificationSettingRepository = gamificationSettingRepository;
			_permissionProvider = permissionProvider;
		}

		public IList<TeamGamificationSettingViewModel> GetTeamGamificationSettingViewModels(List<Guid> siteIds)
		{
			var teamIds = getTeamIds(siteIds);
			var teams = _teamRepository.FindTeams(teamIds);

			var vmList = teams.Select(team =>
			{
				var tgs = _teamGamificationSettingRepository.FindTeamGamificationSettingsByTeam(team);
				return new TeamGamificationSettingViewModel
				{
					Team = new SelectOptionItem
					{
						id = team.Id.Value.ToString(),
						text = team.SiteAndTeam
					},
					GamificationSettingId = tgs?.GamificationSetting.Id.Value ?? Guid.Empty
				};
			})
			.OrderBy(vm => vm.Team.text)
			.ToList();

			return vmList;
		}

		public List<TeamGamificationSettingViewModel> SetTeamsGamificationSetting(TeamsGamificationSettingForm input)
		{
			return input.TeamIds.Select(teamId => SetTeamGamificationSetting(teamId, input.GamificationSettingId)).ToList();
		}

		public TeamGamificationSettingViewModel SetTeamGamificationSetting(Guid teamId, Guid inputGamificationSettingId)
		{
			var team = _teamRepository.Get(teamId);
			if (team == null) return null;

			var gamificationSettingId = Guid.Empty;
			var teamGamificationSetting = _teamGamificationSettingRepository.FindTeamGamificationSettingsByTeam(team);
			if (inputGamificationSettingId == Guid.Empty)
			{
				if (teamGamificationSetting != null)
				{
					_teamGamificationSettingRepository.Remove(teamGamificationSetting);
				}
			}
			else
			{
				var gamificationSetting = _gamificationSettingRepository.Get(inputGamificationSettingId);
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

			return new TeamGamificationSettingViewModel
			{
				GamificationSettingId = gamificationSettingId,
				Team = new SelectOptionItem { id = team.Id.ToString(), text = team.Site.Description.Name + "/" + team.Description.Name }
			};
		}

		private IEnumerable<Guid> getTeamIds(IEnumerable<Guid> siteIds)
		{
			return siteIds
				.SelectMany(id => _teamRepository
						.FindTeamsForSite(id)
						.Where(t => _permissionProvider.HasTeamPermission(functionPath, DateOnly.Today, t))
				)
				.Select(t => t.Id.Value);
		}
	}
}