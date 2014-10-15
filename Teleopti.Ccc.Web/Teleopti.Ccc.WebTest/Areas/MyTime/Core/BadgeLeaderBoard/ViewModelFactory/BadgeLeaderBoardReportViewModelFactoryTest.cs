using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.BadgeLeaderBoardReport;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MyReport;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.BadgeLeaderBoardReport.ViewModelFactory
{
	[TestFixture]
	public class BadgeLeaderBoardReportViewModelFactoryTest
	{
		[Test]
		public void ShouldReturnBadgeLeaderBoardViewModel()
		{
			var badgesProvider = MockRepository.GenerateMock<IAgentBadgeProvider>();
			var target = new BadgeLeaderBoardReportViewModelFactory(badgesProvider);
			var list = new List<AgentBadgeOverview>();			
			badgesProvider.Stub(
				x => x.GetPermittedAgents(DateOnly.Today, DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard))
				.Return(list);

			var result = target.CreateBadgeLeaderBoardReportViewModel(DateOnly.Today);

			result.GetType().Should().Be<BadgeLeaderBoardReportViewModel>();
		}

		[Test]
		public void ShouldReturnOrderedAgents()
		{
			var badgesProvider = MockRepository.GenerateMock<IAgentBadgeProvider>();
			var target = new BadgeLeaderBoardReportViewModelFactory(badgesProvider);
			var overview_original = new AgentBadgeOverview[]
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

			var date = DateOnly.Today;

			badgesProvider.Stub(x => x.GetPermittedAgents(date, DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard))
				.Return(overview_original);

			var result = target.CreateBadgeLeaderBoardReportViewModel(date);

			result.Agents.ElementAt(0).AgentName.Should().Equals("cc");
			result.Agents.ElementAt(1).AgentName.Should().Equals("bb");
			result.Agents.ElementAt(2).AgentName.Should().Equals("aa");
			result.Agents.ElementAt(0).AgentName.Should().Equals("ee");
			result.Agents.ElementAt(0).AgentName.Should().Equals("dd");
		}
	}

	
}