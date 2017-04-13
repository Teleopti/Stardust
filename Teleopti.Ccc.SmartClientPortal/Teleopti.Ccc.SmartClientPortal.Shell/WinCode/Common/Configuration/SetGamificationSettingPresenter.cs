using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Common.Configuration
{
	public class SetGamificationSettingPresenter 
	{
		private readonly ISiteProvider _siteProvider;
		private readonly ITeamProvider _teamProvider;
		private readonly IGamificationSettingProvider _gamificationSettingProvider;
		private readonly ISetGamificationSettingView _view;
		private readonly IUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly ITeamGamificationSettingRepository _teamSettingRepository;
		private List<TeamGamificationSettingModel> _teamGamificationSettingList;
		private IList<ITeam> allTeams;
		private IList<ITeamGamificationSetting> allTeamGamificationSettings;

		public SetGamificationSettingPresenter(ISetGamificationSettingView view, IUnitOfWorkFactory currentUnitOfWorkFactory, ITeamGamificationSettingRepository teamSettingRepository, ISiteProvider siteProvider, ITeamProvider teamProvider, IGamificationSettingProvider gamificationSettingProvider)
		{
			_view = view;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_teamSettingRepository = teamSettingRepository;
			_siteProvider = siteProvider;
			_teamProvider = teamProvider;
			_gamificationSettingProvider = gamificationSettingProvider;
			_teamGamificationSettingList = new List<TeamGamificationSettingModel>();
		}

		public void SelectSite(ISite site)
		{
			var teamSettingModelsInSite = new List<TeamGamificationSettingModel>();
			IList<ITeam> teamsInSite = new List<ITeam>();
			if (!_siteProvider.AllSitesItem.Equals(site))
			{
				teamsInSite = allTeams.Where(t => site.Equals(t.Site)).ToList();
			}
			else
			{
				teamsInSite = allTeams;
			}

			foreach (var team in teamsInSite)
			{
				if (_teamGamificationSettingList.Any(x => x.Team.Id == team.Id))
				{
					var editingTeamSetting = _teamGamificationSettingList.Single(x => x.Team.Id == team.Id);
					teamSettingModelsInSite.Add(editingTeamSetting);
				}
				else if (allTeamGamificationSettings.Any(x => x.Team.Id == team.Id))
				{
					var existingTeamSetting = allTeamGamificationSettings.Single(x => x.Team.Id == team.Id);
					teamSettingModelsInSite.Add(new TeamGamificationSettingModel(existingTeamSetting));
				}
				else
				{
					var nullTeamGamificationSettingModel = new TeamGamificationSettingModel(new TeamGamificationSetting { Team = team, GamificationSetting = GamificationSettingProvider.NullGamificationSetting});
					teamSettingModelsInSite.Add(nullTeamGamificationSettingModel);
				}
			}

			_view.SetTeams(teamSettingModelsInSite);
		}

		public void Initialize()
		{
			using (var uow = _currentUnitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var allGamificationSettingsIncludeEmpty = _gamificationSettingProvider.GetGamificationSettingsEmptyIncluded();
				refreshGamificationSettings(allGamificationSettingsIncludeEmpty);
				allTeams = _teamProvider.GetTeams().ToList();
				foreach (var team in allTeams)
				{
					LazyLoadingManager.Initialize(team.Site);
				}
				allTeamGamificationSettings = _teamSettingRepository.FindAllTeamGamificationSettingsSortedByTeam().ToList();
				foreach (var teamGamificationSetting in allTeamGamificationSettings)
				{
					LazyLoadingManager.Initialize(teamGamificationSetting.Team);
					LazyLoadingManager.Initialize(teamGamificationSetting.GamificationSetting);
				}
				var allSites = _siteProvider.GetSitesAllSitesItemIncluded();
				_view.SetSites(allSites);
				_view.SetSelectedSite(allSites[0]);
			}
		}

		private void refreshGamificationSettings(IEnumerable<IGamificationSetting> settings )
		{
			_view.SetGamificationSettings(settings);
		}

		public void SaveChanges()
		{
			using (var uow = _currentUnitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				foreach (var settingModel in _teamGamificationSettingList)
				{
					if (!settingModel.Id.HasValue && settingModel.GamificationSetting.Description.Name != "No gamification setting")
					{
						_teamSettingRepository.Add(settingModel.ContainedEntity);
					}
					else if (settingModel.Id.HasValue && settingModel.GamificationSetting.Description.Name != "No gamification setting")
					{
						uow.Merge(settingModel.ContainedEntity);
					}
					else if (settingModel.Id.HasValue && settingModel.GamificationSetting.Description.Name == "No gamification setting")
					{
						_teamSettingRepository.Remove(settingModel.ContainedOriginalEntity);
					}
				}
				uow.PersistAll();
			}
		}

		public void UpdateTeamGamificationSettings(IList<TeamGamificationSettingModel> teamGamificationSettingModels)
		{
			_teamGamificationSettingList = teamGamificationSettingModels.Union(_teamGamificationSettingList, new TeamGamificationSettingComparer()).ToList();
		}
	}

	public class TeamGamificationSettingComparer : IEqualityComparer<TeamGamificationSettingModel>
	{
		public bool Equals(TeamGamificationSettingModel x, TeamGamificationSettingModel y)
		{
			if (ReferenceEquals(x, y)) return true;
			if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
				return false;
			return x.Team.Id == y.Team.Id;
		}

		public int GetHashCode(TeamGamificationSettingModel obj)
		{
			if (ReferenceEquals(obj, null)) return 0;
			int hashTeamId = obj.Team.Id.GetHashCode();
			return hashTeamId;
		}
	}
}
