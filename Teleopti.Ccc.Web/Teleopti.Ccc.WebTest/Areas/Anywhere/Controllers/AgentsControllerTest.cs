using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure;
using Teleopti.Ccc.Web.Areas.Anywhere.Controllers;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Rta;

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

			using (var target = new StubbingControllerBuilder().CreateController<AgentsController>(permissionProvider, teamRepository, null, date,null))
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
				               PersonId = Guid.NewGuid(),
				               State = "out of adherence",
				               Activity = "Phone",
				               NextActivity = "Lunch",
				               NextActivityStartTime = new DateTime(2001, 1, 1, 12, 3, 0),
				               Alarm = "Alarma!",
				               AlarmTime = new DateTime(2001, 1, 1, 12, 0, 0),
							   AlarmColor = ColorTranslator.ToHtml(Color.Red)
			               };

			var dataReader = new fakeStateReader(new List<AgentAdherenceStateInfo>()
			                                     {
				                                     expected,
													 expected
			                                     });

			using (var target = new StubbingControllerBuilder().CreateController<AgentsController>(new FakePermissionProvider(), MockRepository.GenerateStub<ITeamRepository>(),null, new Now(), dataReader))
			{
			
				var result = target.GetStates(teamId).Data as IEnumerable<AgentAdherenceStateInfo>;

				result.Count().Should().Be(2);

				Assert.That(result.First(),Is.EqualTo(expected));
				Assert.That(result.Last(),Is.EqualTo(expected));
			}
		
		}

		[Test]
		public void GetAgents_ShouldGetAllAgentsForOneTeam()
		{
			var teamId = Guid.NewGuid();
			var team = new Team();
			team.SetId(teamId);
			var person = new Person();
			var personId = Guid.NewGuid();
			person.SetId(personId);
			person.Name= new Name("bill","gates");
			var teamRepository = MockRepository.GenerateMock<ITeamRepository>();
			teamRepository.Stub(x => x.Get(teamId)).Return(team);
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			var today = new Now();
			var period = new DateOnlyPeriod(today.LocalDateOnly(), today.LocalDateOnly());
			personRepository.Stub(x => x.FindPeopleBelongTeam(team, period)).Return(new List<IPerson>{person});
			using (var target = new StubbingControllerBuilder().CreateController<AgentsController>(new FakePermissionProvider(), teamRepository, personRepository, new Now(), null))
			{
				var expected = new AgentViewModel {Id = personId.ToString(), Name = person.Name.ToString()};
				var result = target.ForTeam(teamId).Data as IEnumerable<AgentViewModel>;

				result.Count().Should().Be(1);

				Assert.That(result.Single().Id, Is.EqualTo(expected.Id));
				Assert.That(result.Single().Name, Is.EqualTo(expected.Name));
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
