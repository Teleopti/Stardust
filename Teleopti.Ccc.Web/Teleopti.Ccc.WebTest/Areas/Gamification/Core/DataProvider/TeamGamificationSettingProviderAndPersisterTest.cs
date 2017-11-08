using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Gamification.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.Gamification.Models;

namespace Teleopti.Ccc.WebTest.Areas.Gamification.Core.DataProvider
{
	[TestFixture]
	class TeamGamificationSettingProviderAndPersisterTest
	{
		[Test]
		public void ShouldAddNewTeamGamificationSettingIfItDoNotExist()
		{
			var teamId = Guid.NewGuid();
			var team = new Team();
			team.WithId(teamId);
			var gamificationSettingId = Guid.NewGuid();
			var gamificationSetting = new GamificationSetting("bla");
			gamificationSetting.WithId(gamificationSettingId);

			var teamRepository = MockRepository.GenerateMock<ITeamRepository>();
			teamRepository.Stub(x => x.Get(teamId)).Return(team);
			var gamificationSettingRepository = MockRepository.GenerateMock<IGamificationSettingRepository>();
			gamificationSettingRepository.Stub(x => x.Get(gamificationSettingId)).Return(gamificationSetting);

			var teamGamificationSettingRepository = new FakeTeamGamificationSettingRepository();
			
			var target = new TeamGamificationSettingProviderAndPersister(teamGamificationSettingRepository, teamRepository, gamificationSettingRepository);

			target.SetTeamGamificationSetting(new TeamGamificationSettingForm(){GamificationSettingId = gamificationSettingId, TeamId = teamId });

			teamGamificationSettingRepository.FindTeamGamificationSettingsByTeam(team).Should().Not.Be.Null();
		}
	}
}
