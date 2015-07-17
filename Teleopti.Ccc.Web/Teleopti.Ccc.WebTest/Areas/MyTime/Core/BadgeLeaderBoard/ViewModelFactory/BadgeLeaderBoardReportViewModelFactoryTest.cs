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

			var overview_original = new[]
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
				.Return(overview_original);

			var target = new BadgeLeaderBoardReportViewModelFactory(toggleManager, leaderboardSettingBasedBadgeProvider);
			var result = target.CreateBadgeLeaderBoardReportViewModel(option);

			result.Agents.ElementAt(0).AgentName.Should().Equals("cc");
			result.Agents.ElementAt(1).AgentName.Should().Equals("bb");
			result.Agents.ElementAt(2).AgentName.Should().Equals("aa");
			result.Agents.ElementAt(0).AgentName.Should().Equals("ee");
			result.Agents.ElementAt(0).AgentName.Should().Equals("dd");
		}

	}

	
}