using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Configuration
{
	[TestFixture]
	public class SetGamificationSettingPresenterTest 
	{
		private IUnitOfWork _unitOfWork;
		private ITeamProvider _teamProvider;
		private IGamificationSettingProvider _settingProvider;
		private ISetGamificationSettingView _view;
		private SetGamificationSettingPresenter _target;
		private ISiteProvider _siteProvider;
		private MockRepository _mocks;
		private ITeamGamificationSettingRepository _teamSettingRepository;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private IList<IGamificationSetting> _gamificationSettings = new List<IGamificationSetting> { new MockRepository().StrictMock<GamificationSetting>() };

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_unitOfWork = _mocks.StrictMock<IUnitOfWork>();

			_unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
			_teamProvider = _mocks.StrictMock<ITeamProvider>();
			_settingProvider = _mocks.StrictMock<IGamificationSettingProvider>();
			_siteProvider = _mocks.StrictMock<ISiteProvider>();
			_view = _mocks.StrictMock<ISetGamificationSettingView>();
			_teamSettingRepository = _mocks.StrictMock<ITeamGamificationSettingRepository>();

			_target = new SetGamificationSettingPresenter(_view, _unitOfWorkFactory, _teamSettingRepository,_siteProvider,_teamProvider, _settingProvider);
		}

		[Test]
		public void VerifyInitialize()
		{
			var site =SiteFactory.CreateSimpleSite("selectedSite");
			var sites = new List<ISite> { site };
			var teams = new List<ITeam> {TeamFactory.CreateSimpleTeam()};
			
			using (_mocks.Record())
			{
				ExpectInitialize(sites, teams, site, new List<ITeamGamificationSetting>());
			}
			using (_mocks.Playback())
			{
				_target.Initialize();
			}
		}

		private void ExpectInitialize(List<ISite> sites, List<ITeam> teams, ISite selectedSite, List<ITeamGamificationSetting> teamSettings )
		{
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Repeat.Any().Return(_unitOfWork);
			Expect.Call(_teamSettingRepository.FindAllTeamGamificationSettingsSortedByTeam())
				.Return(teamSettings);
			Expect.Call(_settingProvider.GetGamificationSettingsEmptyIncluded()).Return(_gamificationSettings);
			Expect.Call(_siteProvider.GetSitesAllSitesItemIncluded()).Return(sites);
			Expect.Call(_teamProvider.GetTeams()).Return(teams);
			Expect.Call(() => _view.SetSites(sites));
			Expect.Call(() => _view.SetSelectedSite(selectedSite));
			Expect.Call(() => _view.SetGamificationSettings(_gamificationSettings));
			Expect.Call(_unitOfWork.Dispose).Repeat.Any();
		}

		[Test]
		public void VerifySelectSite()
		{
			var site = _mocks.StrictMock<ISite>();
			var allSitesSite = _mocks.StrictMock<ISite>();
			var team = _mocks.StrictMock<ITeam>();
			var teams = new List<ITeam> { team };
			using (_mocks.Record())
			{
				ExpectInitialize(new List<ISite> { allSitesSite, site }, teams, allSitesSite, new List<ITeamGamificationSetting>());
				Expect.Call(_siteProvider.AllSitesItem).Return(allSitesSite);
				Expect.Call(team.Site).Repeat.Twice().Return(site);
				Expect.Call(() => _view.SetTeams(new List<TeamGamificationSettingModel>())).IgnoreArguments();
				Expect.Call(site.Equals(site)).Return(true);
				Expect.Call(allSitesSite.Equals(site)).Return(false);
			}
			using (_mocks.Playback())
			{
				_target.Initialize();
				_target.SelectSite(site);
			}
		}

		[Test]
		public void VerifySelectAllSites()
		{
			var allSitesSite = _mocks.StrictMock<ISite>();
			var site = _mocks.StrictMock<ISite>();
			var team = _mocks.StrictMock<ITeam>();
			var teams = new List<ITeam> {team};
			using (_mocks.Record())
			{
				ExpectInitialize(new List<ISite> { allSitesSite, site }, teams, allSitesSite, new List<ITeamGamificationSetting>());
				Expect.Call(team.Site).Return(site);
				Expect.Call(_siteProvider.AllSitesItem).Return(allSitesSite);
				Expect.Call(() => _view.SetTeams(new List<TeamGamificationSettingModel>())).IgnoreArguments();
				Expect.Call(allSitesSite.Equals(allSitesSite)).Return(true);
			}
			using (_mocks.Playback())
			{
				_target.Initialize();
				_target.SelectSite(allSitesSite);
			}
		}

		[Test]
		public void VerifyCanSaveChanges()
		{
			var selectedSite = SiteFactory.CreateSimpleSite("selectedSite");
			var sites = new List<ISite> { selectedSite };

			var teamWithNewSetting = TeamFactory.CreateSimpleTeam("teamNewSetting");
			teamWithNewSetting.SetId(Guid.NewGuid());
			var teamToBeUpdated = TeamFactory.CreateSimpleTeam("teamUpdatedSetting");
			teamToBeUpdated.SetId(Guid.NewGuid());
			var teamToBeDeleted = TeamFactory.CreateSimpleTeam("teamDeletedSetting");
			teamToBeDeleted.SetId(Guid.NewGuid());
			var teams = new List<ITeam> { teamWithNewSetting, teamToBeUpdated, teamToBeDeleted };

			var newTeamSetting = new TeamGamificationSetting { Team = teamWithNewSetting, GamificationSetting = new GamificationSetting("newSetting") };
			var teamSettingToBeRemoved = new TeamGamificationSetting { Team = teamToBeDeleted, GamificationSetting = new GamificationSetting("No gamification setting") };
			teamSettingToBeRemoved.SetId(Guid.NewGuid());
			var teamSettingToToBeUpdated = new TeamGamificationSetting{Team = teamToBeUpdated,GamificationSetting = new GamificationSetting("toBeUpdated")};
			teamSettingToToBeUpdated.SetId(Guid.NewGuid());

			var newTeamSettingModel = new TeamGamificationSettingModel(newTeamSetting);
			var teamSettingToBeRemovedModel = new TeamGamificationSettingModel(teamSettingToBeRemoved);
			var teamSettingToBeUpdatedModel = new TeamGamificationSettingModel(teamSettingToToBeUpdated);

			var teamSettingList = new List<ITeamGamificationSetting>{teamSettingToToBeUpdated,teamSettingToBeRemoved};

			using (_mocks.Record())
			{
				ExpectInitialize(sites, teams, selectedSite, teamSettingList);
				Expect.Call(() => _teamSettingRepository.Add(newTeamSettingModel.ContainedEntity));
				Expect.Call(() => _teamSettingRepository.Remove(teamSettingToBeRemovedModel.ContainedEntity));
				Expect.Call(_unitOfWork.Merge(teamSettingToBeUpdatedModel.ContainedEntity)).Return(teamSettingToToBeUpdated.EntityClone());
				Expect.Call(_unitOfWork.PersistAll()).Return(new List<IRootChangeInfo>()); 

			}
			using (_mocks.Playback())
			{
				_target.Initialize();
				_target.UpdateTeamGamificationSettings(new List<TeamGamificationSettingModel>{newTeamSettingModel, teamSettingToBeRemovedModel, teamSettingToBeUpdatedModel});
				_target.SaveChanges();
			}
		}
	}
}
