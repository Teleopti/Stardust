using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure;
using Teleopti.Ccc.Infrastructure.Rta;
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

		[Test]
		public void GetStates_ShouldGetAllTheStatesForTeamFromReader()
		{
			var teamId = Guid.NewGuid();
			var expected = new AgentAdherenceStateInfo()
			               {
				               Name = "Ashley Andeen",
				               State = "out of adherence",
				               Activity = "Phone",
				               NextActivity = "Lunch",
				               NextActivityStartTime = new DateTime(2001, 1, 1, 12, 3, 0),
				               Alarm = "Alarma!",
				               AlarmTime = new DateTime(2001, 1, 1, 12, 0, 0)
			               };

			var dataReader = new fakeStateReader(new List<AgentAdherenceStateInfo>()
			                                     {
				                                     expected,
													 expected
			                                     });

			using (var target = new StubbingControllerBuilder().CreateController<AgentsController>(new FakePermissionProvider(), MockRepository.GenerateStub<ITeamRepository>(), new Now(), dataReader))
			{
			
				var result = target.GetStates(teamId).Data as IEnumerable<AgentAdherenceStateInfo>;

				result.Count().Should().Be(2);

				Assert.That(result.First(),Is.EqualTo(expected));
				Assert.That(result.Last(),Is.EqualTo(expected));
			}
		
		}

		private class fakeStateReader : IAgentStateReader
		{
			private readonly IEnumerable<AgentAdherenceStateInfo> _statesForAnyTeam;

			public fakeStateReader(IEnumerable<AgentAdherenceStateInfo> statesForAnyTeam)
			{
				_statesForAnyTeam = statesForAnyTeam;
			}

			public IEnumerable<AgentAdherenceStateInfo> GetLatestStatesForTeam(Guid teamId)
			{
				return _statesForAnyTeam;
			}
		}
	}
}
