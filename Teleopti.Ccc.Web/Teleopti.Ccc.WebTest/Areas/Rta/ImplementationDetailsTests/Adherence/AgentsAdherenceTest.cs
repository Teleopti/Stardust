using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Rta;

namespace Teleopti.Ccc.WebTest.Areas.Rta.ImplementationDetailsTests.Adherence
{
	public class AgentsAdherenceTest
	{
		[Test]
		public void ShouldSendOutAllAgentsStatesInOneTeam()
		{
			var personId = Guid.NewGuid();
			var agentState = actualAgentState(personId);

			var expected = new AgentsAdherenceMessage
			{
				AgentStates = new List<AgentAdherenceStateInfo> { new AgentAdherenceStateInfo
				{
					PersonId = agentState.PersonId,
					Activity = agentState.Scheduled,
					Alarm = agentState.AlarmName,
					AlarmColor = ColorTranslator.ToHtml(Color.FromArgb(agentState.Color)),
					AlarmStart = agentState.AlarmStart,
					NextActivity = agentState.ScheduledNext,
					NextActivityStartTime = agentState.NextStart,
					State = agentState.State,
					StateStart = agentState.StateStart
				} } 
			};

			var broker = new FakeMessageSender();
			var organizationForPerson = MockRepository.GenerateMock<IOrganizationForPerson>();
			organizationForPerson.Stub(x => x.GetOrganization(Guid.Empty)).IgnoreArguments()
				.Return(new PersonOrganizationData { TeamId = Guid.NewGuid(), PersonId = personId});
			var target = new AdherenceAggregator(broker, organizationForPerson);

			target.Aggregate(agentState);

			broker.LastAgentsNotification.DeserializeBindaryData<AgentsAdherenceMessage>().AgentStates.Single().Should().Be(expected.AgentStates.Single());
		}

		[Test]
		public void ShouldSendBatchOfAgentsWhenAboveFortyInOneTeam()
		{
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid(); 
			var fiftyOneTeamMates = Enumerable.Range(0, 50).Select(_ => Guid.NewGuid()).Concat(new []{personId}).ToArray();

			var broker = new FakeMessageSender();
			var organizationForPerson = new OrganizationForPerson(new PersonOrganizationProvider(new FakeOrganizationReader(teamId,fiftyOneTeamMates)));
			var target = new AdherenceAggregator(broker, organizationForPerson);
			fiftyOneTeamMates.ForEach(i=>target.Initialize(actualAgentState(i)));
			target.Aggregate(actualAgentState(personId));

			broker.LastAgentsNotification.DeserializeBindaryData<AgentsAdherenceMessage>().AgentStates.Count().Should().Be.EqualTo(11);
		}

		private IActualAgentState actualAgentState(Guid personId)
		{
			return new ActualAgentState
			{
				State = "Ready",
				StateStart = DateTime.Now,
				AlarmId = Guid.NewGuid(),
				AlarmName = "Adherencing",
				AlarmStart = DateTime.Now,
				Color = Color.Red.ToArgb(),
				NextStart = DateTime.Now,
				PersonId = personId,
				Scheduled = "Phone",
				ScheduledNext = "Lunch"
			};
		}
	}

	public class FakeOrganizationReader : IPersonOrganizationReader
	{
		private readonly Guid _teamId;
		private readonly IEnumerable<Guid> _teamMates;

		public FakeOrganizationReader(Guid teamId, IEnumerable<Guid> teamMates)
		{
			_teamId = teamId;
			_teamMates = teamMates;
		}

		public IEnumerable<PersonOrganizationData> PersonOrganizationData()
		{
			return _teamMates.Select(t => new PersonOrganizationData {PersonId = t, TeamId = _teamId});
		}
	}
}
