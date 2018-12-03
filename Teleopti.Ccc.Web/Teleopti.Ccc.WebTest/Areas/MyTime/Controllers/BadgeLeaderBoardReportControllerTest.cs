using System;
using System.Collections.Generic;
using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Gamification.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.BadgeLeaderBoardReport.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.BadgeLeaderBoardReport;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	public class BadgeLeaderBoardReportControllerTest
	{
		[Test]
		public void Index_WhenUserHasPermissionForBadgeLeaderBoardReport_ShouldReturnPartialView()
		{
			var viewModelFactory = MockRepository.GenerateMock<IBadgeLeaderBoardReportViewModelFactory>();
			var timezone = new FakeUserTimeZone();
			var target = new BadgeLeaderBoardReportController(viewModelFactory, null, null, null, timezone, null);
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
			var gamificationSettingProvider = MockRepository.GenerateMock<IGamificationSettingProvider>();
			var controller = new BadgeLeaderBoardReportController(null, null, null, null, new FakeUserTimeZone(), gamificationSettingProvider);
			controller.Index().Should().Be.OfType<ViewResult>();
		}

		public class ShouldCreateLeaderboardOptionWithOneTeamPermitted :OptionsForLeaderboardTest
		{
			protected override void Assert(dynamic result)
			{
				dynamic data = result.Data;
				(data.options[0].text as string).Should().Be.EqualTo("Everyone");
				(data.options[1].text as string).Should().Be.EqualTo("site");
				(data.options[2].text as string).Should().Be.EqualTo("team");
			}
		}

		public class ShouldCreateLeaderboardOptionWithMyTeamPermitted:OptionsForLeaderboardTest
		{
			protected override void Assert(dynamic result)
			{
				dynamic data = result.Data;
				(data.options[0].text as string).Should().Be.EqualTo("Everyone");
				(data.options[1].text as string).Should().Be.EqualTo("team");
			}

			protected override bool IsSitePermitted
			{
				get { return false; }
			}
		}
		
		public class ShouldHaveMyTeamAsDefaultOptionId : OptionsForLeaderboardTest
		{
			protected override void Assert(dynamic result)
			{
				dynamic data = result.Data;
				((Guid)data.defaultOptionId).Should().Be.EqualTo(Guid.Parse("235D8D6D-E46E-44D7-B9EA-92D7B85BE49E"));
			}
		}

		public class ShouldHaveEveryoneAsDefaultOptionIdWhenNotPermittedForMyOwnTeam:OptionsForLeaderboardTest
		{
			protected override void Assert(dynamic result)
			{
				dynamic data = result.Data;
				((Guid)data.defaultOptionId).Should().Be.EqualTo(Guid.Empty);
			}

			protected override IList<ITeam> PermittedTeams(ITeam team)
			{
				return new ITeam[]{};
			}
		}

		public class ShouldHaveEveryoneAsDefaultOptionIdWhenIDontBelongToAnyTeam : OptionsForLeaderboardTest
		{
			protected override void Assert(dynamic result)
			{
				dynamic data = result.Data;
				((Guid)data.defaultOptionId).Should().Be.EqualTo(Guid.Empty);
			}

			protected override IPerson CurrentUser(Team team)
			{
				return PersonFactory.CreatePersonWithId();
			}
		}

		public abstract class OptionsForLeaderboardTest
		{
			private DateOnly date = DateOnly.Today;
			private const string raptorMytimewebViewbadgeleaderboard = DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard;

			[Test]
			public void RunTest()
			{
				var target = setUpControllerForOrganizationalOptions();

				var result = target.OptionsForLeaderboard();

				Assert(result);
			}

			protected abstract void Assert(dynamic result);

			protected virtual IPerson CurrentUser(Team team)
			{
				return PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(2000, 1, 1), team);
			}

			protected virtual IList<ITeam> PermittedTeams(ITeam team)
			{
				return new[] {team};
			}

			protected virtual bool IsSitePermitted
			{
				get { return true; }
			}

			private BadgeLeaderBoardReportController setUpControllerForOrganizationalOptions()
			{
				var team = createTeam();
				var teamProvider = MockRepository.GenerateMock<ITeamProvider>();

				var now = MockRepository.GenerateMock<INow>();
				now.Stub(x => x.UtcDateTime()).Return(date.Date);

				teamProvider.Stub(x => x.GetPermittedTeams(date, raptorMytimewebViewbadgeleaderboard)).Return(PermittedTeams(team));

				var principalAuthorization = MockRepository.GenerateMock<IAuthorization>();
				principalAuthorization.Stub(x => x.IsPermitted(raptorMytimewebViewbadgeleaderboard, date, team.Site)).Return(IsSitePermitted);

				var currentLoggedOnUser = new FakeLoggedOnUser(CurrentUser(team)); 
				var factory = new BadgeLeaderBoardReportOptionFactory(teamProvider, principalAuthorization, currentLoggedOnUser);

				return new BadgeLeaderBoardReportController(null, factory, null, now, new FakeUserTimeZone(), null);
			}

			private static Team createTeam()
			{
				var team = new Team();
				team.SetId(Guid.Parse("235D8D6D-E46E-44D7-B9EA-92D7B85BE49E"));
				team.SetDescription(new Description("team"));
				team.Site = new Site("site");
				return team;
			}
		}
	}
}