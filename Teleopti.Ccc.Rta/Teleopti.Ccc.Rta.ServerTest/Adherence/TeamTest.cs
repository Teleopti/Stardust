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
	public class TeamTest
	{
		[Test]
		public void ShouldAggregateAdherenceFor2PersonsInATeam()
		{
			var outOfAdherence1 = new ActualAgentState { StaffingEffect = 1, PersonId = Guid.NewGuid() };
			var outOfAdherence2 = new ActualAgentState { StaffingEffect = 1, PersonId = Guid.NewGuid() };

			var broker = new MessageSenderExposingLastNotification();
			var teamProvider = MockRepository.GenerateMock<ITeamIdForPerson>();
			var team = Guid.NewGuid();
			teamProvider.Expect(x => x.GetTeamId(outOfAdherence1.PersonId)).Return(team);
			teamProvider.Expect(x => x.GetTeamId(outOfAdherence2.PersonId)).Return(team);
			var target = new AdherenceAggregator(broker, teamProvider, null);

			target.Invoke(outOfAdherence1);
			target.Invoke(outOfAdherence2);

			broker.LastNotification.GetOriginal<TeamAdherenceMessage>().OutOfAdherence.Should().Be(2);
		}

		[Test]
		public void ShouldAggregateAdherenceFor2PersonsInDifferentTeam()
		{
			var outOfAdherence1 = new ActualAgentState { StaffingEffect = 1, PersonId = Guid.NewGuid() };
			var outOfAdherence2 = new ActualAgentState { StaffingEffect = 1, PersonId = Guid.NewGuid() };

			var broker = new MessageSenderExposingLastNotification();
			var teamProvider = MockRepository.GenerateMock<ITeamIdForPerson>();
			teamProvider.Expect(x => x.GetTeamId(outOfAdherence1.PersonId)).Return(Guid.NewGuid());
			teamProvider.Expect(x => x.GetTeamId(outOfAdherence2.PersonId)).Return(Guid.NewGuid());
			var target = new AdherenceAggregator(broker, teamProvider, null);

			target.Invoke(outOfAdherence1);
			target.Invoke(outOfAdherence2);

			broker.LastNotification.GetOriginal<TeamAdherenceMessage>().OutOfAdherence.Should().Be(1);
		}
	}
}
