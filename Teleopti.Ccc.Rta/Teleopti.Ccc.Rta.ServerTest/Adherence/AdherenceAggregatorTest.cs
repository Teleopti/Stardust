using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Rta.Server.Adherence;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Messaging.SignalR;

namespace Teleopti.Ccc.Rta.ServerTest.Adherence
{
	public class AdherenceAggregatorTest
	{
		[Test]
		public void ShouldSendMessageForTeam()
		{
			var agentState = new ActualAgentState();
			var teamId = Guid.NewGuid();

			var broker = new MessageSenderExposingLastNotification();
			var teamProvider = MockRepository.GenerateMock<ITeamIdForPersonProvider>();
			var target = new AdherenceAggregator(broker, teamProvider);

			teamProvider.Expect(x => x.GetTeamId(agentState.PersonId)).Return(teamId);

			target.Invoke(agentState);

			broker.LastNotification.GetOriginal<TeamAdherenceMessage>().TeamId.Should().Be(teamId);
		}

		[Test]
		public void ShouldNotSendMessageForTeamIfAdherenceHasNotChanged()
		{
			var oldState = new ActualAgentState {StaffingEffect = 1};
			var newState = new ActualAgentState {StaffingEffect = 1};

			var broker = MockRepository.GenerateMock<IMessageSender>();
			var teamProvider = MockRepository.GenerateMock<ITeamIdForPersonProvider>();
			var target = new AdherenceAggregator(broker, teamProvider);
			
			target.Invoke(oldState);
			target.Invoke(newState);

			broker.AssertWasCalled(x => x.SendNotification(null), a => a.IgnoreArguments().Repeat.Once());
		}

		[Test]
		public void ShouldMapOutOfAdherenceBasedOnPositiveStaffingEffect()
		{
			var inAdherence = new ActualAgentState { StaffingEffect = 0 };
			var outOfAdherence = new ActualAgentState { StaffingEffect = 1 };

			var broker = new MessageSenderExposingLastNotification();
			var teamProvider = MockRepository.GenerateMock<ITeamIdForPersonProvider>();
			var target = new AdherenceAggregator(broker, teamProvider);

			target.Invoke(inAdherence);
			target.Invoke(outOfAdherence);

			broker.LastNotification.GetOriginal<TeamAdherenceMessage>().OutOfAdherence.Should().Be(1);
		}
		
		[Test]
		public void ShouldMapOutOfAdherenceBasedOnNegativeStaffingEffect()
		{
			var inAdherence = new ActualAgentState { StaffingEffect = 0 };
			var outOfAdherence = new ActualAgentState { StaffingEffect = -1 };

			var broker = new MessageSenderExposingLastNotification();
			var teamProvider = MockRepository.GenerateMock<ITeamIdForPersonProvider>();
			var target = new AdherenceAggregator(broker, teamProvider);

			target.Invoke(inAdherence);
			target.Invoke(outOfAdherence);

			broker.LastNotification.GetOriginal<TeamAdherenceMessage>().OutOfAdherence.Should().Be(1);
		}
	}
}
