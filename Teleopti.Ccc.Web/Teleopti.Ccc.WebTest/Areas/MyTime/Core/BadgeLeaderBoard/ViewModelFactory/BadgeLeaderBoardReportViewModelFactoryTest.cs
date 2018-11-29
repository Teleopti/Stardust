using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.BadgeLeaderBoardReport.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.BadgeLeaderBoardReport;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.BadgeLeaderBoard.ViewModelFactory
{
	[TestFixture]
	public class BadgeLeaderBoardReportViewModelFactoryTest
	{
		[Test]
		public void ShouldGetLeaderBoardWithinPeriod()
		{
			var option = new LeaderboardQuery
			{
				Date = DateOnly.Today,
				SelectedId = Guid.NewGuid(),
				Type = LeadboardQueryType.Team,
				StartDate = new DateOnly(2017,10,1),
				EndDate = new DateOnly(2017,10,31)
			};
			var period = new DateOnlyPeriod(option.StartDate.Value, option.EndDate.Value);
			var leaderboardSettingBasedBadgeProvider = MockRepository.GenerateMock<ILeaderboardSettingBasedBadgeProvider>();
			leaderboardSettingBasedBadgeProvider.Stub(
					x => x.PermittedAgentBadgeOverviewsForTeam(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, option, period))
				.Return(new List<AgentBadgeOverview>(){new AgentBadgeOverview(){AgentName = "aa", Bronze = 0, Silver = 1, Gold = 0}});

			var target = new BadgeLeaderBoardReportViewModelFactory(leaderboardSettingBasedBadgeProvider);

			var result = target.CreateBadgeLeaderBoardReportViewModel(option);
			result.Agents.First().AgentName.Should().Be.EqualTo("aa");
			result.Agents.First().Silver.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldReturnBadgeLeaderBoardViewModel()
		{
			var option = new LeaderboardQuery
			{
				Date = DateOnly.Today,
				SelectedId = Guid.NewGuid(),
				Type = LeadboardQueryType.Team
			};

			var leaderboardSettingBasedBadgeProvider = MockRepository.GenerateMock<ILeaderboardSettingBasedBadgeProvider>();
			leaderboardSettingBasedBadgeProvider.Stub(
				x => x.PermittedAgentBadgeOverviewsForTeam(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, option, new DateOnlyPeriod(new DateOnly(1900, 1, 1), DateOnly.Today)))
				.Return(new List<AgentBadgeOverview>());

			var target = new BadgeLeaderBoardReportViewModelFactory(leaderboardSettingBasedBadgeProvider);

			var result = target.CreateBadgeLeaderBoardReportViewModel(option);
			result.GetType().Should().Be<BadgeLeaderBoardReportViewModel>();
		}

		[Test]
		public void ShouldReturnSortedAgents()
		{
			var overviewOriginal = new[]
			{
				new AgentBadgeOverview
				{
					AgentName = "aa",
					Gold = 30,
					Silver = 24,
					Bronze = 12
				},
				new AgentBadgeOverview
				{
					AgentName = "bb",
					Gold = 30,
					Silver = 26,
					Bronze = 12
				},
				new AgentBadgeOverview
				{
					AgentName = "cc",
					Gold = 32,
					Silver = 24,
					Bronze = 11
				},
				new AgentBadgeOverview
				{
					AgentName = "dd",
					Gold = 28,
					Silver = 23,
					Bronze = 15
				},
				new AgentBadgeOverview
				{
					AgentName = "ee",
					Gold = 28,
					Silver = 23,
					Bronze = 18
				}
			};

			var option = new LeaderboardQuery
			{
				Date = DateOnly.Today,
				SelectedId = Guid.NewGuid(),
				Type = LeadboardQueryType.Site
			};

			var leaderboardSettingBasedBadgeProvider = MockRepository.GenerateMock<ILeaderboardSettingBasedBadgeProvider>();
			leaderboardSettingBasedBadgeProvider.Stub(
				x => x.PermittedAgentBadgeOverviewsForSite(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, option, new DateOnlyPeriod(new DateOnly(1900, 1, 1), DateOnly.Today)))
				.Return(overviewOriginal);

			var target = new BadgeLeaderBoardReportViewModelFactory(leaderboardSettingBasedBadgeProvider);
			var agentBadges = target.CreateBadgeLeaderBoardReportViewModel(option).Agents.ToList();

			agentBadges.ElementAt(0).AgentName.Should().Be("cc");
			agentBadges.ElementAt(1).AgentName.Should().Be("bb");
			agentBadges.ElementAt(2).AgentName.Should().Be("aa");
			agentBadges.ElementAt(3).AgentName.Should().Be("ee");
			agentBadges.ElementAt(4).AgentName.Should().Be("dd");
		}
	}
}