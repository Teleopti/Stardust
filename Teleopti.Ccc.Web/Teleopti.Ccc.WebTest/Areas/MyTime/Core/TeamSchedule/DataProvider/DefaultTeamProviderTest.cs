using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.TeamSchedule.DataProvider
{
	[TestFixture]
	public class DefaultTeamProviderTest
	{
		[Test]
		public void ShouldDefaultToMyTeam()
		{
			var myTeam = new Team();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(createPersonWithTeam(myTeam));
			var target = new DefaultTeamProvider(loggedOnUser, new FakePermissionProvider(), null);

			var actual = target.DefaultTeam(DateOnly.Today);

			actual.Should().Be(myTeam);
		}
		
		[Test]
		public void ShouldDefaultToFirstAvailableTeamIfMyTeamNotPermitted()
		{
			var myTeam = new Team();
			var otherTeams = new[] { new Team(), new Team() };
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var teamProvider = MockRepository.GenerateMock<ITeamProvider>();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(createPersonWithTeam(myTeam));
			permissionProvider.Stub(x => x.HasTeamPermission(DefinedRaptorApplicationFunctionPaths.TeamSchedule, DateOnly.Today, myTeam)).Return(false);
			teamProvider.Stub(x => x.GetPermittedTeams(DateOnly.Today, DefinedRaptorApplicationFunctionPaths.TeamSchedule)).Return(otherTeams);
			var target = new DefaultTeamProvider(loggedOnUser, permissionProvider, teamProvider);

			var actual = target.DefaultTeam(DateOnly.Today);

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
			loggedOnUser.Stub(x => x.CurrentUser()).Return(createPersonWithTeam(myTeam));
			permissionProvider.Stub(x => x.HasTeamPermission(DefinedRaptorApplicationFunctionPaths.TeamSchedule, DateOnly.Today, myTeam)).Return(false);
			teamProvider.Stub(x => x.GetPermittedTeams(DateOnly.Today, DefinedRaptorApplicationFunctionPaths.TeamSchedule)).Return(otherTeams);
			var target = new DefaultTeamProvider(loggedOnUser, permissionProvider, teamProvider);

			var actual = target.DefaultTeam(DateOnly.Today);

			actual.Should().Be(myTeam);
		}

		[Test]
		public void ShouldDefaultToFirstPermittedTeamWhenNoOwnTeam()
		{
			var otherTeams = new[] { new Team(), new Team() };
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var teamProvider = MockRepository.GenerateMock<ITeamProvider>();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(new Person());
			permissionProvider.Stub(x => x.HasTeamPermission(DefinedRaptorApplicationFunctionPaths.TeamSchedule, DateOnly.Today, null)).Return(true);
			teamProvider.Stub(x => x.GetPermittedTeams(DateOnly.Today, DefinedRaptorApplicationFunctionPaths.TeamSchedule)).Return(otherTeams);
			var target = new DefaultTeamProvider(loggedOnUser, permissionProvider, teamProvider);

			var actual = target.DefaultTeam(DateOnly.Today);

			actual.Should().Be(otherTeams.ElementAt(0));
		}

		private IPerson createPersonWithTeam(ITeam team)
		{
			var person = new Person();
			person.AddPersonPeriod(new PersonPeriod(DateOnly.Today.AddDays(-10), new PersonContract(new Contract("sd"), new PartTimePercentage("d"), new ContractSchedule("d")), team));
			return person;
		}
	}
}