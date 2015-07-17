using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.MyTime.Core.BadgeLeaderBoardReport.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.BadgeLeaderBoardReport;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.BadgeLeaderBoard.ViewModelFactory
{
	[TestFixture]
	public class BadgeLeaderBoardReportViewModelFactoryTest
	{
		[Test]
		public void ShouldReturnBadgeLeaderBoardViewModel()
		{
			var toggleManager = MockRepository.GenerateMock<IToggleManager>();
			var leaderboardSettingBasedBadgeProvider =MockRepository.GenerateMock<ILeaderboardSettingBasedBadgeProvider>();

			var target = new BadgeLeaderBoardReportViewModelFactory(toggleManager,leaderboardSettingBasedBadgeProvider);
			var option = new LeaderboardQuery
			{
				Date = DateOnly.Today,
				SelectedId = Guid.NewGuid(),
				Type = LeadboardQueryType.Team
			};

			var result = target.CreateBadgeLeaderBoardReportViewModel(option);

			result.GetType().Should().Be<BadgeLeaderBoardReportViewModel>();
		}

		[Test]
		public void ShouldReturnSortedAgents()
		{
			var toggleManager = MockRepository.GenerateMock<IToggleManager>();
			toggleManager.Stub(x => x.IsEnabled(Toggles.Portal_DifferentiateBadgeSettingForAgents_31318)).Return(true);

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
				x => x.PermittedAgentBadgeOverviewsForSite(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, option))
				.Return(overviewOriginal);

			var target = new BadgeLeaderBoardReportViewModelFactory(toggleManager, leaderboardSettingBasedBadgeProvider);
			var agentBadges = target.CreateBadgeLeaderBoardReportViewModel(option).Agents.ToList();

			agentBadges.ElementAt(0).AgentName.Should().Be("cc");
			agentBadges.ElementAt(1).AgentName.Should().Be("bb");
			agentBadges.ElementAt(2).AgentName.Should().Be("aa");
			agentBadges.ElementAt(3).AgentName.Should().Be("ee");
			agentBadges.ElementAt(4).AgentName.Should().Be("dd");
		}

	}

	
}