﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.Gamification.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.Gamification.Models;

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
		private TeamGamificationSettingForm _teamGamificationSettingForm;

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
			_gamificationSettingRepository = MockRepository.GenerateMock<IGamificationSettingRepository>();
			_gamificationSettingRepository.Stub(x => x.Get(gamificationSettingId)).Return(_gamificationSetting);
			_teamGamificationSettingRepository = new FakeTeamGamificationSettingRepository();
			_teamGamificationSettingForm = new TeamGamificationSettingForm()
			{
				GamificationSettingId = _gamificationSetting.Id.Value,
				TeamId = _team.Id.Value
			};
		}

		[Test]
		public void ShouldAddNewTeamGamificationSettingIfItDoNotExist()
		{
			var target = new TeamGamificationSettingProviderAndPersister(_teamGamificationSettingRepository, _teamRepository, _gamificationSettingRepository);

			target.SetTeamGamificationSetting(_teamGamificationSettingForm);

			_teamGamificationSettingRepository.FindTeamGamificationSettingsByTeam(_team).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldModifyGamificationSetting()
		{
			_teamGamificationSettingRepository.Add(new TeamGamificationSetting(){GamificationSetting = new GamificationSetting("old"), Team = _team});
			var target = new TeamGamificationSettingProviderAndPersister(_teamGamificationSettingRepository, _teamRepository, _gamificationSettingRepository);

			var result = target.SetTeamGamificationSetting(_teamGamificationSettingForm);

			result.GamificationSettingId.Should().Be.EqualTo(_teamGamificationSettingForm.GamificationSettingId);
		}

		[Test]
		public void ShouldReturnNullWhenHasNoSuchTeam()
		{
			var teamRepository = new FakeTeamRepository();
			var target = new TeamGamificationSettingProviderAndPersister(_teamGamificationSettingRepository, teamRepository, _gamificationSettingRepository);

			var result = target.SetTeamGamificationSetting(_teamGamificationSettingForm);

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldReturnNullWhenHasNoSuchGamificationSetting()
		{
			var gamificationSettingRepository = new FakeGamificationSettingRepository();
			var target = new TeamGamificationSettingProviderAndPersister(_teamGamificationSettingRepository, _teamRepository, gamificationSettingRepository);

			var result = target.SetTeamGamificationSetting(_teamGamificationSettingForm);

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldRemoveExistTeamGamificationSettingWhenInputEmptyId()
		{
			_teamGamificationSettingRepository.Add(new TeamGamificationSetting() { GamificationSetting = _gamificationSetting, Team = _team });
			var target = new TeamGamificationSettingProviderAndPersister(_teamGamificationSettingRepository, _teamRepository, _gamificationSettingRepository);

			_teamGamificationSettingForm.GamificationSettingId = Guid.Empty;
			target.SetTeamGamificationSetting(_teamGamificationSettingForm);

			_teamGamificationSettingRepository.LoadAll().Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnEmptyListWhenThereIsNoTeam()
		{
			var target = new TeamGamificationSettingProviderAndPersister(_teamGamificationSettingRepository, _teamRepository, _gamificationSettingRepository);
			var result = target.GetTeamGamificationSettingViewModels(new List<Guid>());
			result.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetTeamGamificationSettings()
		{
			_teamGamificationSettingRepository.Add(new TeamGamificationSetting() { GamificationSetting = _gamificationSetting, Team = _team });
			var target = new TeamGamificationSettingProviderAndPersister(_teamGamificationSettingRepository, _teamRepository, _gamificationSettingRepository);

			var result = target.GetTeamGamificationSettingViewModels(new List<Guid>(){_team.Id.Value});

			result.Count.Should().Be.EqualTo(1);
			result[0].GamificationSettingId.Should().Be.EqualTo(_gamificationSetting.Id);
			result[0].Team.id.Should().Be.EqualTo(_team.Id.ToString());
		}

		[Test]
		public void ShouldGetEmptyIdWhenThereIsNoTeamGamificationSetting()
		{
			var target = new TeamGamificationSettingProviderAndPersister(_teamGamificationSettingRepository, _teamRepository, _gamificationSettingRepository);

			var result = target.GetTeamGamificationSettingViewModels(new List<Guid>() { _team.Id.Value });

			result.Count.Should().Be.EqualTo(1);
			result[0].GamificationSettingId.Should().Be.EqualTo(Guid.Empty);
			result[0].Team.id.Should().Be.EqualTo(_team.Id.ToString());
		}
	}
}
