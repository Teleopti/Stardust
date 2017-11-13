using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.Gamification.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.Gamification.Models;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Gamification.Core.DataProvider
{
	[TestFixture]
	class TeamGamificationSettingProviderAndPersisterTest
	{
		private IGamificationSettingRepository _gamificationSettingRepository;
		private IGamificationSetting _gamificationSetting;
		private ITeamGamificationSettingRepository _teamGamificationSettingRepository;
		private ITeamRepository _teamRepository;
		private ITeam _team;
		private ISiteProvider _siteProvider;
		private ISite _site;

		[SetUp]
		public void Setup()
		{
			var teamId = Guid.NewGuid();
			_team = new Team();
			_team.WithId(teamId);
			var gamificationSettingId = Guid.NewGuid();
			_gamificationSetting = new GamificationSetting("bla");
			_gamificationSetting.WithId(gamificationSettingId);

			_teamRepository = new FakeTeamRepository();
			_teamRepository.Add(_team);

			var siteId = Guid.NewGuid();
			_site = new Site("site");
			_site.WithId(siteId);
			_site.AddTeam(_team);
			_siteProvider = MockRepository.GenerateMock<ISiteProvider>();
			_siteProvider.Stub(x=>x.GetPermittedTeamsUnderSite(siteId, DateOnly.Today, DefinedRaptorApplicationFunctionPaths.OpenOptionsPage)).Return(new List<ITeam>{_team});

			_gamificationSettingRepository = MockRepository.GenerateMock<IGamificationSettingRepository>();
			_gamificationSettingRepository.Stub(x => x.Get(gamificationSettingId)).Return(_gamificationSetting);
			_teamGamificationSettingRepository = new FakeTeamGamificationSettingRepository();
		}

		[Test]
		public void ShouldAddNewTeamGamificationSettingIfItDoNotExist()
		{
			var target = new TeamGamificationSettingProviderAndPersister(_teamGamificationSettingRepository, _teamRepository, _gamificationSettingRepository, _siteProvider);

			target.SetTeamGamificationSetting(_team.Id.Value, _gamificationSetting.Id.Value);

			_teamGamificationSettingRepository.FindTeamGamificationSettingsByTeam(_team).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldModifyGamificationSetting()
		{
			_teamGamificationSettingRepository.Add(new TeamGamificationSetting(){GamificationSetting = new GamificationSetting("old"), Team = _team});
			var target = new TeamGamificationSettingProviderAndPersister(_teamGamificationSettingRepository, _teamRepository, _gamificationSettingRepository, _siteProvider);

			var result = target.SetTeamGamificationSetting(_team.Id.Value, _gamificationSetting.Id.Value);

			result.GamificationSettingId.Should().Be.EqualTo(_gamificationSetting.Id.Value);
		}

		[Test]
		public void ShouldReturnNullWhenHasNoSuchTeam()
		{
			var teamRepository = new FakeTeamRepository();
			var target = new TeamGamificationSettingProviderAndPersister(_teamGamificationSettingRepository, teamRepository, _gamificationSettingRepository, _siteProvider);

			var result = target.SetTeamGamificationSetting(_team.Id.Value, _gamificationSetting.Id.Value);

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldReturnNullWhenHasNoSuchGamificationSetting()
		{
			var gamificationSettingRepository = new FakeGamificationSettingRepository();
			var target = new TeamGamificationSettingProviderAndPersister(_teamGamificationSettingRepository, _teamRepository, gamificationSettingRepository, _siteProvider);

			var result = target.SetTeamGamificationSetting(_team.Id.Value, _gamificationSetting.Id.Value);

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldRemoveExistTeamGamificationSettingWhenInputEmptyId()
		{
			_teamGamificationSettingRepository.Add(new TeamGamificationSetting() { GamificationSetting = _gamificationSetting, Team = _team });
			var target = new TeamGamificationSettingProviderAndPersister(_teamGamificationSettingRepository, _teamRepository, _gamificationSettingRepository, _siteProvider);

			target.SetTeamGamificationSetting(_team.Id.Value, Guid.Empty);

			_teamGamificationSettingRepository.LoadAll().Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnEmptyListWhenThereIsNoTeam()
		{
			var target = new TeamGamificationSettingProviderAndPersister(_teamGamificationSettingRepository, _teamRepository, _gamificationSettingRepository, _siteProvider);
			var result = target.GetTeamGamificationSettingViewModels(new List<Guid>());
			result.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetTeamGamificationSettings()
		{
			_teamGamificationSettingRepository.Add(new TeamGamificationSetting() { GamificationSetting = _gamificationSetting, Team = _team });
			var target = new TeamGamificationSettingProviderAndPersister(_teamGamificationSettingRepository, _teamRepository, _gamificationSettingRepository, _siteProvider);

			var result = target.GetTeamGamificationSettingViewModels(new List<Guid>(){ _site.Id.Value});

			result.Count.Should().Be.EqualTo(1);
			result[0].GamificationSettingId.Should().Be.EqualTo(_gamificationSetting.Id);
			result[0].Team.id.Should().Be.EqualTo(_team.Id.ToString());
		}

		[Test]
		public void ShouldGetEmptyIdWhenThereIsNoTeamGamificationSetting()
		{
			var target = new TeamGamificationSettingProviderAndPersister(_teamGamificationSettingRepository, _teamRepository, _gamificationSettingRepository, _siteProvider);

			var result = target.GetTeamGamificationSettingViewModels(new List<Guid>() { _site.Id.Value });

			result.Count.Should().Be.EqualTo(1);
			result[0].GamificationSettingId.Should().Be.EqualTo(Guid.Empty);
			result[0].Team.id.Should().Be.EqualTo(_team.Id.ToString());
		}

		[Test]
		public void ShouldSetTeamsGamificationSetting()
		{
			_teamGamificationSettingRepository.Add(new TeamGamificationSetting() { GamificationSetting = new GamificationSetting("old"), Team = _team });
			var target = new TeamGamificationSettingProviderAndPersister(_teamGamificationSettingRepository, _teamRepository, _gamificationSettingRepository, _siteProvider);

			var result = target.SetTeamsGamificationSetting(new TeamsGamificationSettingForm()
			{
				TeamIds = new List<Guid>() { _team.Id.Value, Guid.Empty },
				GamificationSettingId = _gamificationSetting.Id.Value
			});

			result.Count.Should().Be.EqualTo(2);
			result[0].GamificationSettingId.Should().Be.EqualTo(_gamificationSetting.Id.Value);
			result[1].Should().Be.EqualTo(null);
		}
	}
}
