using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Rta.Server.Adherence;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Rta;
using Teleopti.Messaging.SignalR;

namespace Teleopti.Ccc.Rta.ServerTest.Adherence
{
	public class AgentsAdherenceTest
	{
		[Test]
		public void ShouldSendOutAllAgentsStatesInOneTeam()
		{
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var person = new Person();
			person.SetId(personId);
			person.Name = new Name(" "," ");
			var alarmId = Guid.NewGuid();
			var actualAgentState = new ActualAgentState()
			{
				State = "Ready",
				StateStart = DateTime.Now,
				AlarmId = alarmId,
				AlarmName = "Adherencing",
				AlarmStart = DateTime.Now,
				Color = Color.Red.ToArgb(),
				NextStart = DateTime.Now,
				PersonId = personId,
				Scheduled = "Phone",
				ScheduledNext = "Lunch"
			};

			var expected = new AgentsAdherenceMessage()
			{
				AgentStates = new List<AgentAdherenceStateInfo> { new AgentAdherenceStateInfo
				{
					PersonId = actualAgentState.PersonId,
					Activity = actualAgentState.Scheduled,
					Alarm = actualAgentState.AlarmName,
					AlarmColor = ColorTranslator.ToHtml(Color.FromArgb(actualAgentState.Color)),
					AlarmStart = actualAgentState.AlarmStart,
					NextActivity = actualAgentState.ScheduledNext,
					NextActivityStartTime = actualAgentState.NextStart,
					State = actualAgentState.State,
					StateStart = actualAgentState.StateStart
				} } 
			};

			var broker = new MessageSenderExposingNotifications();
			var organizationForPerson = MockRepository.GenerateMock<IOrganizationForPerson>();
			organizationForPerson.Stub(x => x.GetOrganization(Guid.Empty)).IgnoreArguments()
				.Return(new PersonOrganizationData { TeamId = teamId, PersonId = personId});
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			personRepository.Stub(x => x.Get(personId)).Return(person);
			var target = new AdherenceAggregator(broker, organizationForPerson);

			target.Invoke(actualAgentState);

			broker.LastAgentsNotification.GetOriginal<AgentsAdherenceMessage>().AgentStates.Single().Should().Be(expected.AgentStates.Single());
		}
	}
}
