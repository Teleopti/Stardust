using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Ccc.WebTest.Core.Portal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.TeamSchedule.DataProvider
{
	[TestFixture]
	public class DefaultTeamCalculatorTest
	{
		[Test]
		public void ShouldDefaultToMyTeam()
		{
			var myTeam = new Team();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggedOnUser.Stub(x => x.MyTeam(DateOnly.Today)).Return(myTeam);
			var target = new DefaultTeamCalculator(loggedOnUser, new FakePermissionProvider(), null);

			var actual = target.Calculate(DateOnly.Today);

			actual.Should().Be(myTeam);
		}

		[Test]
		public void ShouldDefaultToFirstAvailableTeamIfMyTeamNotPermitted()
		{
			var myTeam = new Team();
			var otherTeams = new[] {new Team(), new Team()};
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var teamProvider = MockRepository.GenerateMock<ITeamProvider>();
			loggedOnUser.Stub(x => x.MyTeam(DateOnly.Today)).Return(myTeam);
			permissionProvider.Stub(x => x.HasTeamPermission(DefinedRaptorApplicationFunctionPaths.TeamSchedule, DateOnly.Today, myTeam)).Return(false);
			teamProvider.Stub(x => x.GetPermittedTeams(DateOnly.Today)).Return(otherTeams);
			var target = new DefaultTeamCalculator(loggedOnUser, permissionProvider, teamProvider);

			var actual = target.Calculate(DateOnly.Today);

			actual.Should().Be(otherTeams.ElementAt(0));
		}

		[Test]
		public void ShouldDefaultToMyTeamIfNoAvailableTeamEvenThoughMyTeamNotPermitted()
		{
			var myTeam = new Team();
			var otherTeams = new ITeam[] {};
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var teamProvider = MockRepository.GenerateMock<ITeamProvider>();
			loggedOnUser.Stub(x => x.MyTeam(DateOnly.Today)).Return(myTeam);
			permissionProvider.Stub(x => x.HasTeamPermission(DefinedRaptorApplicationFunctionPaths.TeamSchedule, DateOnly.Today, myTeam)).Return(false);
			teamProvider.Stub(x => x.GetPermittedTeams(DateOnly.Today)).Return(otherTeams);
			var target = new DefaultTeamCalculator(loggedOnUser, permissionProvider, teamProvider);

			var actual = target.Calculate(DateOnly.Today);

			actual.Should().Be(myTeam);
		}

		[Test]
		public void ShouldDefaultToFirstPermittedTeamWhenNoOwnTeam()
		{
			var otherTeams = new[] { new Team(), new Team() };
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var teamProvider = MockRepository.GenerateMock<ITeamProvider>();
			loggedOnUser.Stub(x => x.MyTeam(DateOnly.Today)).Return(null);
			permissionProvider.Stub(x => x.HasTeamPermission(DefinedRaptorApplicationFunctionPaths.TeamSchedule, DateOnly.Today, null)).Return(true);
			teamProvider.Stub(x => x.GetPermittedTeams(DateOnly.Today)).Return(otherTeams);
			var target = new DefaultTeamCalculator(loggedOnUser, permissionProvider, teamProvider);

			var actual = target.Calculate(DateOnly.Today);

			actual.Should().Be(otherTeams.ElementAt(0));
		}

	}
}