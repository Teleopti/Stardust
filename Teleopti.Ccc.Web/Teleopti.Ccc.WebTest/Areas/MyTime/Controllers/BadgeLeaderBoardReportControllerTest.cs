using System;
using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.BadgeLeaderBoardReport.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.BadgeLeaderBoardReport;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	public class BadgeLeaderBoardReportControllerTest
	{
		[Test]
		public void Index_WhenUserHasPermissionForBadgeLeaderBoardReport_ShouldReturnPartialView()
		{
			var viewModelFactory = MockRepository.GenerateMock<IBadgeLeaderBoardReportViewModelFactory>();
			var target = new BadgeLeaderBoardReportController(viewModelFactory, null, null, null);
			var model = new BadgeLeaderBoardReportViewModel();
			var option = new LeaderboardQuery
			{
				Date = DateOnly.Today,
				SelectedId = Guid.Empty,
				Type = LeadboardQueryType.Everyone
			};

			viewModelFactory.Stub(x => x.CreateBadgeLeaderBoardReportViewModel(option)).Return(model);

			var result = target.Overview(option);

			result.Data.Should().Be.SameInstanceAs(model);
		}

		[Test]
		public void IndexShouldReturnPartialView()
		{
			var controller = new BadgeLeaderBoardReportController(null, null, null, null);
			controller.Index().Should().Be.OfType<ViewResult>();
		}

		[Test]
		public void ShouldCreateLeaderboardOptionWithOneTeamPermitted()
		{
			var target = setUpControllerForOrganizationalOptions();

			var result = target.OptionsForLeaderboard();

			dynamic data = result.Data;
			(data.options[0].text as string).Should().Be.EqualTo("Everyone");
			(data.options[1].text as string).Should().Be.EqualTo("site");
			(data.options[2].text as string).Should().Be.EqualTo("team");
		}

		[Test]
		public void ShouldCreateLeaderboardOptionWithMyTeamPermitted()
		{
			var target = setUpControllerForOrganizationalOptions(false);

			var result = target.OptionsForLeaderboard();

			dynamic data = result.Data;
			(data.options[0].text as string).Should().Be.EqualTo("Everyone");
			(data.options[1].text as string).Should().Be.EqualTo("team");
		}

		[Test]
		public void ShouldHaveMyTeamAsDefaultOptionId()
		{
			var target = setUpControllerForOrganizationalOptions();

			var result = target.OptionsForLeaderboard();

			dynamic data = result.Data;
			((Guid)data.defaultOptionId).Should().Be.EqualTo(Guid.Parse("235D8D6D-E46E-44D7-B9EA-92D7B85BE49E"));
		}

		private static BadgeLeaderBoardReportController setUpControllerForOrganizationalOptions(bool isSitePermitted = true)
		{
			var team = new Team();
			team.SetId(Guid.Parse("235D8D6D-E46E-44D7-B9EA-92D7B85BE49E"));
			team.Description = new Description("team");
			team.Site = new Site("site");
			var teamProvider = MockRepository.GenerateMock<ITeamProvider>();
			const string viewbadgeleaderboard = DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard;

			var dateOnly = DateOnly.Today;
			var now = MockRepository.GenerateMock<INow>();
			now.Stub(x => x.UtcDateTime()).Return(dateOnly);

			teamProvider.Stub(x => x.GetPermittedTeams(dateOnly, viewbadgeleaderboard)).Return(new[] { team });

			var principalAuthorization = MockRepository.GenerateMock<IPrincipalAuthorization>();
			principalAuthorization.Stub(x => x.IsPermitted(viewbadgeleaderboard, dateOnly, team.Site)).Return(isSitePermitted);

			var currentLoggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			currentLoggedOnUser.Stub(x => x.CurrentUser()).Return(PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(2000, 1, 1), team));
			var factory = new BadgeLeaderBoardReportOptionFactory(teamProvider, principalAuthorization, currentLoggedOnUser);

			return new BadgeLeaderBoardReportController(null, factory, null, now);
		}
	}
}