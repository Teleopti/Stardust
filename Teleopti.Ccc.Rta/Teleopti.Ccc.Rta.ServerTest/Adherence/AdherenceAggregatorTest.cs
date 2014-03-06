using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Rta.Server.Adherence;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
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
	}
}
