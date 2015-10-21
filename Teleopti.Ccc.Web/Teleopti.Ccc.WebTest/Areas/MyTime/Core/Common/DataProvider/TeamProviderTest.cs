using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.Global.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Common.DataProvider
{
	[TestFixture]
	public class TeamProviderTest
	{
		[Test]
		public void ShouldQueryAllTeams()
		{
			var repository = MockRepository.GenerateMock<ITeamRepository>();
			var target = new TeamProvider(repository, MockRepository.GenerateMock<IPermissionProvider>());

			target.GetPermittedTeams(DateOnly.Today, DefinedRaptorApplicationFunctionPaths.TeamSchedule);

			repository.AssertWasCalled(x => x.FindAllTeamByDescription());
		}

		[Test]
		public void ShouldFilterPermittedTeamsWhenQueryingAll()
		{
			var repository = MockRepository.GenerateMock<ITeamRepository>();
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var teams = new ITeam[] { new Domain.AgentInfo.Team(), new Domain.AgentInfo.Team() };

			repository.Stub(x => x.FindAllTeamByDescription()).Return(teams);
			permissionProvider.Stub(x => x.HasTeamPermission(DefinedRaptorApplicationFunctionPaths.TeamSchedule, DateOnly.Today, teams.ElementAt(0))).Return(false);
			permissionProvider.Stub(x => x.HasTeamPermission(DefinedRaptorApplicationFunctionPaths.TeamSchedule, DateOnly.Today, teams.ElementAt(1))).Return(true);

			var target = new TeamProvider(repository, permissionProvider);

			var result = target.GetPermittedTeams(DateOnly.Today, DefinedRaptorApplicationFunctionPaths.TeamSchedule);

			result.Single().Should().Be(teams.ElementAt(1));
		}

	}
}