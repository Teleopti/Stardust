using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.Gamification.Controller;
using Teleopti.Ccc.Web.Areas.Gamification.Models;

namespace Teleopti.Ccc.WebTest.Areas.Gamification
{
	[TestFixture]
	[GamificationTest]
	public class GamificationControllerTeamTest
	{
		public FakeGamificationSettingRepository GamificationSettingRepository;
		public FakeTeamGamificationSettingRepository TeamGamificationSettingRepository;
		public FakeTeamRepository TeamRepository;
		public FakeSiteRepository SiteRepository;
		public GamificationController Target;
		
		[Test]
		public void ShouldAddNewTeamGamificationSettingIfItDoNotExist()
		{
			var team = new Team().WithId();
			var gamificationSetting = new GamificationSetting("bla").WithId();
			var site = new Site("site").WithId();
			site.AddTeam(team);

			SiteRepository.Add(site);
			TeamRepository.Add(team);

			GamificationSettingRepository.Add(gamificationSetting);

			Target.SetTeamGamificationSetting(new TeamsGamificationSettingForm
			{
				TeamIds = new List<Guid> {team.Id.Value},
				GamificationSettingId = gamificationSetting.Id.Value
			});

			TeamGamificationSettingRepository.FindTeamGamificationSettingsByTeam(team).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldModifyGamificationSetting()
		{
			var team = new Team().WithId();
			var gamificationSetting = new GamificationSetting("bla").WithId();
			var site = new Site("site").WithId();
			site.AddTeam(team);

			SiteRepository.Add(site);
			TeamRepository.Add(team);

			GamificationSettingRepository.Add(gamificationSetting);
			TeamGamificationSettingRepository.Add(new TeamGamificationSetting {GamificationSetting = new GamificationSetting("old").WithId(), Team = team});

			var result = Target.SetTeamGamificationSetting(new TeamsGamificationSettingForm
			{
				TeamIds = new List<Guid> { team.Id.Value },
				GamificationSettingId = gamificationSetting.Id.Value
			});

			result.First().GamificationSettingId.Should().Be.EqualTo(gamificationSetting.Id.Value);
		}

		[Test]
		public void ShouldReturnNullWhenHasNoSuchTeam()
		{
			var team = new Team().WithId();
			var gamificationSetting = new GamificationSetting("bla").WithId();
			var site = new Site("site").WithId();
			site.AddTeam(team);

			SiteRepository.Add(site);
			TeamRepository.Add(team);

			GamificationSettingRepository.Add(gamificationSetting);
			
			var result = Target.SetTeamGamificationSetting(new TeamsGamificationSettingForm
			{
				TeamIds = new List<Guid> { Guid.NewGuid() },
				GamificationSettingId = gamificationSetting.Id.Value
			});

			result.Single().Should().Be.Null();
		}

		[Test]
		public void ShouldReturnNullWhenHasNoSuchGamificationSetting()
		{
			var team = new Team().WithId();
			var gamificationSetting = new GamificationSetting("bla").WithId();
			var site = new Site("site").WithId();
			site.AddTeam(team);

			SiteRepository.Add(site);
			TeamRepository.Add(team);

			GamificationSettingRepository.Add(gamificationSetting);

			var result = Target.SetTeamGamificationSetting(new TeamsGamificationSettingForm
			{
				TeamIds = new List<Guid> { team.Id.Value },
				GamificationSettingId = Guid.NewGuid()
			});

			result.First().Should().Be.Null();
		}

		[Test]
		public void ShouldRemoveExistTeamGamificationSettingWhenInputEmptyId()
		{
			var team = new Team().WithId();
			var gamificationSetting = new GamificationSetting("bla").WithId();
			var site = new Site("site").WithId();
			site.AddTeam(team);

			SiteRepository.Add(site);
			TeamRepository.Add(team);

			GamificationSettingRepository.Add(gamificationSetting);
			TeamGamificationSettingRepository.Add(new TeamGamificationSetting { GamificationSetting = gamificationSetting, Team = team });
			
			Target.SetTeamGamificationSetting(new TeamsGamificationSettingForm
			{
				TeamIds = new List<Guid> { team.Id.Value },
				GamificationSettingId = Guid.Empty
			});

			TeamGamificationSettingRepository.LoadAll().Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnEmptyListWhenThereIsNoTeam()
		{
			var team = new Team().WithId();
			var gamificationSetting = new GamificationSetting("bla").WithId();
			var site = new Site("site").WithId();
			site.AddTeam(team);

			SiteRepository.Add(site);
			TeamRepository.Add(team);

			GamificationSettingRepository.Add(gamificationSetting);
			var result = Target.GetTeamGamificationSettings(new List<Guid>());
			result.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetTeamGamificationSettings()
		{
			var team = new Team().WithId();
			var gamificationSetting = new GamificationSetting("bla").WithId();
			var site = new Site("site").WithId();
			site.AddTeam(team);

			SiteRepository.Add(site);
			TeamRepository.Add(team);

			GamificationSettingRepository.Add(gamificationSetting);
			TeamGamificationSettingRepository.Add(new TeamGamificationSetting { GamificationSetting = gamificationSetting, Team = team });
			
			var result = Target.GetTeamGamificationSettings(new List<Guid>{ site.Id.Value});

			result.Count().Should().Be.EqualTo(1);
			result.First().GamificationSettingId.Should().Be.EqualTo(gamificationSetting.Id);
			result.First().Team.id.Should().Be.EqualTo(team.Id.ToString());
		}

		[Test]
		public void ShouldGetEmptyIdWhenThereIsNoTeamGamificationSetting()
		{
			var team = new Team().WithId();
			var gamificationSetting = new GamificationSetting("bla").WithId();
			var site = new Site("site").WithId();
			site.AddTeam(team);

			SiteRepository.Add(site);
			TeamRepository.Add(team);

			GamificationSettingRepository.Add(gamificationSetting);

			var result = Target.GetTeamGamificationSettings(new List<Guid> { site.Id.Value });

			result.Count().Should().Be.EqualTo(1);
			result.First().GamificationSettingId.Should().Be.EqualTo(Guid.Empty);
			result.First().Team.id.Should().Be.EqualTo(team.Id.ToString());
		}

		[Test]
		public void ShouldSetTeamsGamificationSetting()
		{
			var team = new Team().WithId();
			var gamificationSetting = new GamificationSetting("bla").WithId();
			var site = new Site("site").WithId();
			site.AddTeam(team);

			SiteRepository.Add(site);
			TeamRepository.Add(team);

			GamificationSettingRepository.Add(gamificationSetting);
			TeamGamificationSettingRepository.Add(new TeamGamificationSetting { GamificationSetting = new GamificationSetting("old"), Team = team });
			
			var result = Target.SetTeamGamificationSetting(new TeamsGamificationSettingForm
			{
				TeamIds = new List<Guid> { team.Id.Value, Guid.Empty },
				GamificationSettingId = gamificationSetting.Id.Value
			});

			result.Count().Should().Be.EqualTo(2);
			result.First().GamificationSettingId.Should().Be.EqualTo(gamificationSetting.Id.Value);
			result.Last().Should().Be.EqualTo(null);
		}
	}
}
