﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Rta;

namespace Teleopti.Ccc.WebTest.Areas.Rta.Core.Server.Adherence
{
	public class AgentsAdherenceTest
	{
		[Test]
		public void ShouldSendOutAllAgentsStatesInOneTeam()
		{
			var personId = Guid.NewGuid();
			var actualAgentState = new ActualAgentState()
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
				.Return(new PersonOrganizationData { TeamId = Guid.NewGuid(), PersonId = personId});
			var target = new AdherenceAggregator(broker, organizationForPerson);

			target.Invoke(actualAgentState);

			broker.LastAgentsNotification.GetOriginal<AgentsAdherenceMessage>().AgentStates.Single().Should().Be(expected.AgentStates.Single());
		}
	}
}
