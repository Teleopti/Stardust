using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.Anywhere.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Controllers
{
	[TestFixture]
	public class AgentsControllerTest
	{
		[Test]
		public void ShouldFailIfNoPermissionForOneTeam()
		{
			var teamId = Guid.NewGuid();
			var team = new Team();
			team.SetId(teamId);
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var teamRepository = MockRepository.GenerateMock<ITeamRepository>();
			var date = new Now();
			permissionProvider.Stub(
				x => x.HasTeamPermission(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, date.LocalDateOnly(),
						team)).Return(false);
			teamRepository.Stub(x => x.Get(teamId)).Return(team);

			using (var target = new StubbingControllerBuilder().CreateController<AgentsController>(permissionProvider, teamRepository, date))
			{
				target.GetStates(teamId);
				target.Response.StatusCode.Should().Be(403);
			}
		}
	}
}
