using System;
using System.Web.Mvc;
using System.Web.Routing;
using MvcContrib.TestHelper.Fakes;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.Anywhere.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Controllers
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture]
	public class PersonControllerTest
	{
		private PersonController target;
		private ITeamProvider teamProvider;
		private ISchedulePersonProvider schedulePersonProvider;
		private ILoggedOnUser _loggedOnUser;

		[SetUp]
		public void Setup()
		{
			teamProvider = MockRepository.GenerateMock<ITeamProvider>();
			schedulePersonProvider = MockRepository.GenerateMock<ISchedulePersonProvider>();
			_loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			target = new PersonController(schedulePersonProvider, teamProvider, _loggedOnUser);
		}

		//[Test]
		//public void ShouldGetTeamsForDate()
		//{
		//	var date = new DateOnly(2012, 12, 01);
		//	var team = TeamFactory.CreateTeam("Team", "Site");
		//	team.SetId(Guid.NewGuid());

		//	teamProvider.Stub(x => x.GetPermittedTeams(date, DefinedRaptorApplicationFunctionPaths.SchedulesAnywhere)).Return(
		//		new[] {team});
		//	target.ControllerContext = new ControllerContext(new FakeHttpContext("/"), new RouteData(), target);

		//	dynamic result = target.AvailableTeams(date.Date).Data;
		//	dynamic teamResult = result.Teams[0];
		//	((object) teamResult.Id).Should().Be.EqualTo(team.Id);
		//	((object) teamResult.SiteAndTeam).Should().Be.EqualTo(team.SiteAndTeam);
		//}

		//[Test]
		//public void ShouldDefaultToMyTeamIfNoTeamAvailable()
		//{
		//	var date = new DateOnly(2012, 12, 01);
		//	var team = TeamFactory.CreateTeam("Team", "Site");
		//	team.SetId(Guid.NewGuid());
		//	var person = MockRepository.GenerateMock<IPerson>();
		//	person.Stub(x => x.MyTeam(date)).Return(team);
		//	_loggedOnUser.Stub(x => x.CurrentUser()).Return(person);

		//	teamProvider.Stub(x => x.GetPermittedTeams(date, DefinedRaptorApplicationFunctionPaths.SchedulesAnywhere)).Return(new ITeam[] { });
		//	target.ControllerContext = new ControllerContext(new FakeHttpContext("/"), new RouteData(), target);

		//	dynamic result = target.AvailableTeams(date.Date).Data;
		//	dynamic teamResult = result.Teams[0];
		//	((object)teamResult.Id).Should().Be.EqualTo(team.Id);
		//	((object)teamResult.SiteAndTeam).Should().Be.EqualTo(team.SiteAndTeam);
		//}

		//[Test]
		//public void ShouldReturnEmptyIfNoTeamAvailableAndNoMyTeam()
		//{
		//	var date = new DateOnly(2012, 12, 01);
		//	var team = TeamFactory.CreateTeam("Team", "Site");
		//	team.SetId(Guid.NewGuid());
		//	var person = MockRepository.GenerateMock<IPerson>();
		//	_loggedOnUser.Stub(x => x.CurrentUser()).Return(person);

		//	teamProvider.Stub(x => x.GetPermittedTeams(date, DefinedRaptorApplicationFunctionPaths.SchedulesAnywhere)).Return(new ITeam[] { });
		//	target.ControllerContext = new ControllerContext(new FakeHttpContext("/"), new RouteData(), target);

		//	dynamic result = target.AvailableTeams(date.Date).Data;
		//	dynamic teamsResult = result.Teams;
		//	((IList)teamsResult).Count.Should().Be.EqualTo(0);
		//}

		[Test]
		public void ShouldGetPeopleForDateAndGroup()
		{
			var date = new DateOnly(2012, 12, 01);
			var team = TeamFactory.CreateTeam("Team", "Site");
			team.SetId(Guid.NewGuid());
			var person = PersonFactory.CreatePerson("anders", "anderson");
			person.SetId(Guid.NewGuid());

			schedulePersonProvider.Stub(
				x =>
				x.GetPermittedPersonsForGroup(date, team.Id.GetValueOrDefault(),
				                             DefinedRaptorApplicationFunctionPaths.SchedulesAnywhere)).Return(new[] {person});
			target.ControllerContext = new ControllerContext(new FakeHttpContext("/"), new RouteData(), target);

			dynamic result = target.PeopleInGroup(date.Date,team.Id.GetValueOrDefault()).Data;
			dynamic teamResult = result[0];
			((object)teamResult.Id).Should().Be.EqualTo(person.Id);
			((object)teamResult.FirstName).Should().Be.EqualTo(person.Name.FirstName);
			((object)teamResult.LastName).Should().Be.EqualTo(person.Name.LastName);
		}

		[TearDown]
		public void Teardown()
		{
			target.Dispose();
		}
	}
}